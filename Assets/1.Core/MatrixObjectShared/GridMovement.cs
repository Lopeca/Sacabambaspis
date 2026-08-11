using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
 
/// <summary>
/// 이동관련 유틸을 제공하는 컴포넌트, GridUpdate를 필요로하는 코드가 아닌 여타 오브젝트 이동 관련 컴포넌트의 이동로직 보조형
/// </summary>
public class GridMovement : MonoBehaviour
{
    private static readonly int Teleport = Animator.StringToHash("Teleport");
 
    private int targetTicks;
    public enum MoveState
    {
        Staying,
        Moving,
        Rolling,
        Falling // 필요 없을 수도
    }
    private Vector2Int startPos;
 
    private Vector2Int destPos;
 
    private MatrixObject mo;
 
    [SerializeField] MoveState state;
    public MoveState State => state;
    [SerializeField] private Vector2Int lastIntendedDirection;
 
    public ObjectPhysicsConfigSO physicsSO;
 
    // 직선 이동(ExecuteMove/PerformMove)은 더 이상 DOTween을 쓰지 않고 코루틴이 직접 보간합니다.
    // moveTween은 구르기(ExecuteRollMove/ExecuteRoll)에서만 사용됩니다.
    private Tween moveTween;
    public Tween MoveTween => moveTween; // 주의: 직선 이동 중에는 항상 null입니다. 구르기 중에만 유효한 값을 가집니다.
    Tween rollTween;
 
    // 현재 "이동 중(직선 또는 구르기)" 여부를 나타내는 단일 플래그.
    // 예전엔 moveTween == null 여부로 진행 상태를 판별했는데, 직선 이동에서 트윈을 없애면서
    // ForceCompleteMove / ForceCancelMove / IsMoveFinished가 기준으로 삼을 값이 필요해져 추가함.
    private bool isActive;
 
    private Coroutine coroutine;
    // 공용 캐싱용 필드
    [SerializeField] private MatrixCell startCell;
    [SerializeField] private MatrixCell destCell;
 
    // 주로 트윈 완료 후 Filled 상태 여지 없이 다음 동작을 수행하도록 할 때
    public event Action AfterOnMoveCompleted;
 
    [Header("Debug Inspector")]
    [SerializeField] private bool trace = false; // 인스펙터에서 켜고 끌 수 있는 체크박스
 
    private bool paused;
    void Awake()
    {
        mo = GetComponent<MatrixObject>();
        state = MoveState.Staying;
 
        targetTicks = Mathf.Max(1, Mathf.RoundToInt(physicsSO.moveDuration / Time.fixedDeltaTime));
    }
 
    /// <summary>
    /// 셀 상태를 직접 관리해서 한 칸 이동시켜주는 함수. 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="targetState"> 오브젝트가 취할 상태 </param>
    /// <param name="destState"> 이동의 의도가 다음 셀의 특이사항으로 반영될 때 </param>
    /// <param name="isPlayerSpeed"></param>
 
    public void ExecuteMove(Vector2Int direction, MoveState targetState, MatrixCell.CellState destState, bool isPlayerSpeed = false)
    {
 
        // 기존에 도르고 있던 코루틴이 있다면 깔끔하게 정돈 후 새로 시작[cite: 1]
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
        }
 
        lastIntendedDirection = direction;
        startPos = new Vector2Int(mo.posX, mo.posY);
        destPos = new Vector2Int(mo.posX + direction.x, mo.posY + direction.y);
        Vector3 destWorldPos = GamePlayGridManager.Instance.GetCell(destPos).transform.position;
 
        state = targetState;
 
        startCell = GamePlayGridManager.Instance.GetCell(startPos);
        destCell = GamePlayGridManager.Instance.GetCell(destPos);
        if (destCell.matrixObject != null)
        {
            // Debug.LogError($"ID : {mo.id} - movent로부터 로직 오류 : 오브젝트가 이미 있는 칸으로의 이동이 감지됨. 이동 가능 여부 검사 로직 확인 필요함.\n" +
            //                $"destCell pos : " + destCell.GetPosition() + "|| destCell Object : " + destCell.matrixObject +"\n" +
            //                "frame : " + Time.frameCount);
            Debug.Break();
            return;
        }
        // 2. 이동에 관여되는 셀들을 잠그고 로직상 이동은 미리 완료함
        mo.GetCurrentCell().moveStateDirection = direction;
        GamePlayGridManager.Instance.MoveMatrixObjectPosition(mo, direction);
        GamePlayGridManager.Instance.SetCellState(startPos, MatrixCell.CellState.Moving);
        GamePlayGridManager.Instance.SetCellState(destPos, destState);
 
        //GamePlayGridManager.Instance.ReserveMove(startPos, destPos, isAttack);
        // 3. 이동이 완료될 때까지 
        PerformMove(destWorldPos, isPlayerSpeed);
    }
 
    /// <summary>
    /// Pusher 컴포넌트에 필요하여 분리된 포지션 이동 담당 함수.
    /// [리팩터] 예전엔 DOTween(SetUpdate(Fixed)) + 코루틴(WaitForFixedUpdate)이라는 두 개의 독립된 시간축이
    /// 같은 이동을 각자 진행시켰습니다. 두 시간축의 위상이 프레임마다 정확히 일치한다는 보장이 없어서,
    /// 특히 "정지 상태에서 새로 이동을 시작하는 첫 이동"에서 트윈 쪽이 코루틴보다 한 틱 먼저 끝나버리는
    /// 어긋남이 생겼습니다(도착 직전/직후 멈칫거림의 원인). 이번 리팩터에서는 트윈을 걷어내고
    /// 코루틴이 매 FixedUpdate마다 직접 위치를 보간하도록 해서, "이동을 진행시키는 주체"를 하나로 통일했습니다.
    /// </summary>
    /// <param name="destWorldPos"></param>
    /// <param name="isPlayerSpeed"></param>
    public void PerformMove(Vector3 destWorldPos, bool isPlayerSpeed = false)
    {
        // 틱 수 계산 (플레이어 틱 or 일반 오브젝트 틱)
        int ticks = isPlayerSpeed ? GamePlayGridManager.Instance.player.MoveTicks : targetTicks;
 
        // 기존 트윈이 겹치지 않도록 Kill 처리 (혹시 PerformMove_CustomCompleteAction 등으로 남아있을 수 있어 방어적으로 정리)
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
        }
        moveTween = null;
 
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
 
        isActive = true;
        coroutine = StartCoroutine(PerformMoveCoroutine(transform.position, destWorldPos, ticks));
    }
 
    IEnumerator PerformMoveCoroutine(Vector3 fromWorldPos, Vector3 toWorldPos, int totalTicks)
    {
        // 슈파플렉스 턴제 박자에 맞춰 exact tick 수만큼 정확히 FixedUpdate를 기다리며, 매 틱마다 위치를 직접 갱신합니다.
        for (int i = 0; i < totalTicks; i++)
        {
            yield return new WaitForFixedUpdate();
 
            if (paused)
            {
                i--; // Pause 시 틱 차감 유예
                continue;
            }
 
            float t = (float)(i + 1) / totalTicks;
            transform.position = Vector3.LerpUnclamped(fromWorldPos, toWorldPos, t);
        }
 
        // 부동소수점 오차 없이 정확히 도착 지점에 스냅
        transform.position = toWorldPos;
 
        // 틱 연산이 끝나는 정확한 순간 논리 상태를 정리합니다.
        CompleteMove();
        state = MoveState.Staying;
        isActive = false;
        coroutine = null;
 
        AfterOnMoveCompleted?.Invoke();
    }
 
    public void PerformMove_CustomCompleteAction(Vector3 destPos, bool isPlayerSpeed = false, Action OnMoveCompleted = null)
    {
        isActive = true;
        moveTween = transform.DOMove(destPos, isPlayerSpeed ? GamePlayGridManager.Instance.playerConfigSO.moveDuration : physicsSO.moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                state = MoveState.Staying;
                isActive = false;
 
                OnMoveCompleted?.Invoke();
                AfterOnMoveCompleted?.Invoke();
            });
    }
 
    void CompleteMove()
    {
        MatrixCell startCell = GamePlayGridManager.Instance.GetCell(startPos);
 
        // 움직이는 사이 폭발 이펙트가 치고 들어올 수 있음 
        if(startCell.matrixObject == null)
            GamePlayGridManager.Instance.SetCellState(startPos, MatrixCell.CellState.Empty);
        GamePlayGridManager.Instance.SetCellState(destPos, MatrixCell.CellState.Filled);
    }
 
    public void ForceCompleteMove()
    {
        if (!isActive) return;
        if (startCell == null) return; // 한번도 움직이지 않은 경우 없을 수 있음
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
 
        if (moveTween != null && moveTween.IsActive()) moveTween.Kill();
        if (rollTween != null && rollTween.IsActive()) rollTween.Kill();
 
        AfterOnMoveCompleted = null;
        isActive = false;
 
        // [핵심] 폭발로 인해 강제 종료될 때는 CompleteMove()를 거쳐 destPos를 Filled로 만드는 대신,
        // 출발지 셀만 깨끗이 비워주고 잔여 트윈을 털어냅니다.
        if (startCell.matrixObject == mo)
        {
            startCell.matrixObject = null;
        }
 
        if (startCell == null)
        {
 
        }
        startCell.state = startCell.matrixObject != null ? MatrixCell.CellState.Filled : MatrixCell.CellState.Empty;
 
        if (trace)
        {
            Debug.Log($"<color=#FF3333>[Frame {Time.frameCount}] startPos : {startPos}, {startCell.state}");
        }
 
        state = MoveState.Staying;
    }
    public void ForceCancelMove()
    {
        if (!isActive) return;
        // 1. 코루틴 및 트윈 즉시 중단
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
 
        if (moveTween != null && moveTween.IsActive()) moveTween.Kill();
        if (rollTween != null && rollTween.IsActive()) rollTween.Kill();
 
        AfterOnMoveCompleted = null;
        isActive = false;
        state = MoveState.Staying;
 
        // 2. [핵심] 자신이 이동을 시작했던 출발지 셀(startPos)의 Moving 상태를 직접 해제
        if (startCell != null && startCell.matrixObject == mo)
        {
            startCell.matrixObject = null;
            startCell.state = MatrixCell.CellState.Empty;
        }
 
        // 3. 내부 좌표 원복 (출발지 위치로)
        if (mo != null)
        {
            mo.posX = startPos.x;
            mo.posY = startPos.y;
 
            MatrixCell originalCell = GamePlayGridManager.Instance.GetCell(startPos);
            if (originalCell != null)
            {
                transform.position = originalCell.transform.position;
            }
 
            destPos = startPos;
        }
    }
 
        // NOTE: 폭발 시스템(ExplodeOnDeath)이나 Pending 처리 로직에서 
        // 셀의 CellState와 matrixObject 참조를 별도로 정리하므로, 
        // 이곳에서 GetCurrentCell().state = CellState.Filled 등의 셀 상태 수정은 절대 하지 않습니다.
 
    // 트윈 도중 
    public void KillTweenOnly()
    {
        moveTween?.Kill();
        rollTween?.Kill();
    }
    public void ForceState(MoveState state)
    {
        this.state = state;
    }
 
    /// <summary>
    /// 이동에 관여하지 않고 회전트윈만 담당하겠다는 뜻
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="isPlayerSpeed"></param>
    /// <summary>
    /// 구르기 전용 통합 실행 함수 (이동 + 회전 트윈 동시 제어)
    /// </summary>
    public void ExecuteRollMove(Vector2Int direction, float speedMultiplier = 0.65f)
    {
        lastIntendedDirection = direction;
        startPos = new Vector2Int(mo.posX, mo.posY);
        destPos = startPos + direction;
        Vector3 destWorldPos = GamePlayGridManager.Instance.GetCell(destPos).transform.position;
 
        state = MoveState.Rolling;
        isActive = true;
 
        startCell = GamePlayGridManager.Instance.GetCell(startPos);
        destCell = GamePlayGridManager.Instance.GetCell(destPos);
 
        // 1. 셀 상태 및 위치 데이터 즉시 반영 (Attacking/Rolling 상태)
        mo.GetCurrentCell().moveStateDirection = direction;
        GamePlayGridManager.Instance.MoveMatrixObjectPosition(mo, direction);
        GamePlayGridManager.Instance.SetCellState(startPos, MatrixCell.CellState.Moving);
        GamePlayGridManager.Instance.SetCellState(destPos, MatrixCell.CellState.Attacking);
 
        // 2. 구르기 전용 Ticks (플레이어 속도보다 speedMultiplier 배율만큼 빠르게)
        int rollTicks = Mathf.Max(1, Mathf.RoundToInt(GamePlayGridManager.Instance.player.MoveTicks * speedMultiplier));
        float rollDuration = rollTicks * Time.fixedDeltaTime;
 
        // 3. 이동 트윈 & 회전 트윈 동시 실행
        moveTween = transform.DOMove(destWorldPos, rollDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Fixed);
 
        float targetAngle = (direction == Vector2Int.left) ? 360f : -360f;
        rollTween = transform.DOLocalRotate(new Vector3(0f, 0f, targetAngle), rollDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(() =>
            {
                transform.localRotation = Quaternion.identity;
                rollTween = null;
            });
 
        // 4. 기존 Pause 검사 로직이 포함된 코루틴 실행
        coroutine = StartCoroutine(RollWithPauseCoroutine(rollTicks, rollDuration));
    }
 
    private IEnumerator RollWithPauseCoroutine(int totalTicks, float totalDuration)
    {
        // A. 구르기 시작 후 약 1/3 지점까지 진행 대기 (틱 기준 또는 시간 기준)
        float pauseWaitTime = totalDuration / 3f;
        yield return new WaitForSeconds(pauseWaitTime);
 
        // B. 바닥이 차있다면 트윈 일시정지 (Pause)
        //    (플레이어가 비키거나 아래 칸이 비어있지 않은 동안 대기)
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Pause();
            rollTween?.Pause();
 
            // 바닥 셀의 matrixObject가 존재하면 비어질 때까지 대기
            while (GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down).matrixObject != null)
            {
                yield return null;
            }
 
            // 바닥이 비었으므로 트윈 재개
            moveTween.Play();
            rollTween?.Play();
        }
 
        // C. 남은 이동 트윈이 완전히 완료될 때까지 대기
        yield return moveTween.WaitForCompletion();
 
        // D. 이동 마감 처리
        CompleteMove();
        state = MoveState.Staying;
        isActive = false;
        coroutine = null;
 
        AfterOnMoveCompleted?.Invoke();
    }
 
    // FSM에서 "이동 끝났나?" 체크용 (다음 상태 전환 조건)
    public bool IsMoveFinished()
    {
        // 직선 이동/구르기 중 어느 쪽이든 진행 중이면 아직 안 끝난 것으로 판단
        if (isActive) return false;
 
        // 혹시 남아있는 트윈(구르기 회전 등)이 있다면 그 상태도 함께 확인
        if (moveTween != null && moveTween.IsActive()) return moveTween.IsComplete();
 
        return true;
    }
    private void OnDestroy()
    {
        // 오브젝트 파괴 시 메모리 누수 방지
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
        }
 
        if (rollTween != null && rollTween.IsActive())
        {
            rollTween.Kill();
        }
 
        if (coroutine != null) StopCoroutine(coroutine);
 
        AfterOnMoveCompleted = null;
    }
 
    public void EnterPipe(Vector2Int direction)
    {
        // 데이터
        startCell = mo.GetCurrentCell();
        destCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + direction * 2);
 
        startCell.state = MatrixCell.CellState.Moving;
        destCell.state = MatrixCell.CellState.Receiving;
 
        destCell.matrixObject = startCell.matrixObject;
        startCell.matrixObject = null;
 
        mo.posX = destCell.GetPosition().x;
        mo.posY = destCell.GetPosition().y;
 
        // 뷰
        StartCoroutine(TeleportCoroutine());
    }
 
    private IEnumerator TeleportCoroutine()
    {
        mo.Animator.Play("Teleport");
        state = MoveState.Moving;
 
        yield return new WaitForSeconds(0.1f);
 
        transform.position = mo.GetCurrentCell().transform.position;
 
        yield return new WaitForSeconds(0.16f);
 
        state = MoveState.Staying;
        mo.Animator.SetBool(Teleport, false);
 
        startCell.state = MatrixCell.CellState.Empty;
        destCell.state = MatrixCell.CellState.Filled;
    }
    /// <summary>
    /// 이동에 관여하지 않고 회전트윈만 담당하겠다는 뜻
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="isPlayerSpeed"></param>
    public void ExecuteRoll(Vector2Int direction, bool isPlayerSpeed = false, bool isPushed = false)
    {
        //duration 동안 한바퀴 회전하는 코드. 왼쪽으로 구르면 반시계, 오른쪽은 시계 방향
        float targetAngle = (direction == Vector2Int.left) ? 360f : -360f;
        float duration = isPlayerSpeed ? GamePlayGridManager.Instance.playerConfigSO.moveDuration : physicsSO.moveDuration;
 
        rollTween = transform.DOLocalRotate(new Vector3(0f, 0f, targetAngle), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 4. 회전이 끝나면 다음 구르기를 위해 로컬 회전값을 깔끔하게 0으로 리셋
                transform.localRotation = Quaternion.identity;
                rollTween = null;
            });
 
        //if (isPushed) return;
        //StartCoroutine(RollCoroutine());
 
    }
    // IEnumerator RollCoroutine()
    // {
    //     yield return new WaitForSeconds(physicsSO.moveDuration / 3);
    //
    //     moveTween.Pause();  // 둘은 DoTween의 트윈들임
    //     rollTween.Pause();
    //     
    //     while (GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down).matrixObject !=
    //            null)
    //     {
    //         
    //         yield return null;
    //     }
    //     
    //     moveTween.Play();
    //     rollTween.Play();
    // }
}