using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 이동관련 유틸을 제공하는 컴포넌트, GridUpdate를 필요로하는 코드가 아닌 여타 오브젝트 이동 관련 컴포넌트의 이동로직 보조형
/// </summary>
public class GridMovement : MonoBehaviour
{
    private static readonly int Teleport = Animator.StringToHash("Teleport");

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
    
    public ObjectPhysicsConfigSO physics;

    private Tween moveTween;
    public Tween MoveTween => moveTween;
    Tween rollTween;

    // 공용 캐싱용 필드
    private MatrixCell startCell;
    private MatrixCell destCell;

    // 주로 트윈 완료 후 Filled 상태 여지 없이 다음 동작을 수행하도록 할 때
    public event Action AfterOnMoveCompleted;
    void Awake()
    {
        mo = GetComponent<MatrixObject>();
        state = MoveState.Staying;
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
        lastIntendedDirection = direction;
        startPos = new Vector2Int(mo.posX, mo.posY);
        destPos = new Vector2Int(mo.posX + direction.x, mo.posY + direction.y);
        Vector3 destWorldPos = GamePlayGridManager.Instance.GetCell(destPos).transform.position;
        
        state = targetState;
        
        MatrixCell destCell = GamePlayGridManager.Instance.GetCell(destPos);
        if (destCell.matrixObject != null)
        {
            Debug.LogError($"ID : {mo.id} - movent로부터 로직 오류 : 오브젝트가 이미 있는 칸으로의 이동이 감지됨. 이동 가능 여부 검사 로직 확인 필요함.\n" +
                           $"destCell pos : " + destCell.GetPosition() + "|| destCell Object : " + destCell.matrixObject +"\n" +
                           "frame : " + Time.frameCount);
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
    /// Pusher 컴포넌트에 필요하여 분리된 포지션 이동 트윈과 트윈중의 오브젝트 상태 관리만을 담당하는 함수
    /// </summary>
    /// <param name="destPos"></param>
    /// <param name="isPlayerSpeed"></param>
    /// <param name="OnMoveCompleted"> 마감처리에 필요한 것 </param>
    public void PerformMove(Vector3 destPos, bool isPlayerSpeed = false)
    {
        moveTween =transform.DOMove(destPos, isPlayerSpeed ? GamePlayGridManager.Instance.playerConfigSO.moveDuration : physics.moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                CompleteMove();
                state = MoveState.Staying;
                
                AfterOnMoveCompleted?.Invoke();
            });
    }
    
    public void PerformMove_CustomCompleteAction(Vector3 destPos, bool isPlayerSpeed = false, Action OnMoveCompleted = null)
    {
        moveTween =transform.DOMove(destPos, isPlayerSpeed ? GamePlayGridManager.Instance.playerConfigSO.moveDuration : physics.moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                state = MoveState.Staying;
                
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
        // 이동 관련 트윈 중 하나라도 살아있고 재생 중이었는지 확인
        bool isMoving = (moveTween != null && moveTween.IsActive() && moveTween.IsPlaying()) 
                        || (rollTween != null && rollTween.IsActive() && rollTween.IsPlaying());

        if (isMoving)
        {
            Debug.Log("[ForceCompleteMove] 진행 중인 이동/구르기 트윈을 강제로 완료시킵니다.");
        }

        AfterOnMoveCompleted = null;
        
        // 트윈 강제 완료 (OnComplete 즉시 호출됨)
        moveTween?.Complete();
        rollTween?.Complete();
    }
    
    public void ForceCancelMove()
    {
        // 이동 관련 트윈 중 하나라도 살아있고 재생 중이었는지 확인
        bool isMoving = (moveTween != null && moveTween.IsActive() && moveTween.IsPlaying()) 
                        || (rollTween != null && rollTween.IsActive() && rollTween.IsPlaying());

        if (isMoving)
        {
            Debug.Log("[ForceCancelMove] 진행 중인 이동/구르기 트윈을 강제로 취소시킵니다.");
        }
        else
        {
            Debug.Log("[ForceCancelMove] 이동/구르기 트윈이 진행중이 아닙니다.");
        }

        AfterOnMoveCompleted = null;
        
        // 트윈 강제 완료 (OnComplete 즉시 호출됨)
        moveTween?.Kill();
        rollTween?.Kill();

        mo.GetCurrentCell().state = MatrixCell.CellState.Empty;
        mo.GetCurrentCell().matrixObject = null;
        
        mo.posX -= lastIntendedDirection.x;
        mo.posY -= lastIntendedDirection.y;
        
        mo.GetCurrentCell().state = MatrixCell.CellState.Filled;
        mo.GetCurrentCell().matrixObject = mo;
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
    public void ExecuteRoll(Vector2Int direction, bool isPlayerSpeed = false, bool isPushed = false)
    {
        //duration 동안 한바퀴 회전하는 코드. 왼쪽으로 구르면 반시계, 오른쪽은 시계 방향
        float targetAngle = (direction == Vector2Int.left) ? 360f : -360f;
        float duration = isPlayerSpeed ? GamePlayGridManager.Instance.playerConfigSO.moveDuration : physics.moveDuration;
        
        rollTween = transform.DOLocalRotate(new Vector3(0f, 0f, targetAngle), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 4. 회전이 끝나면 다음 구르기를 위해 로컬 회전값을 깔끔하게 0으로 리셋
                transform.localRotation = Quaternion.identity;
                rollTween = null;
            });

        if (isPushed) return;
        StartCoroutine(RollCoroutine());
    }

    IEnumerator RollCoroutine()
    {
        yield return new WaitForSeconds(physics.moveDuration / 3);

        moveTween.Pause();  // 둘은 DoTween의 트윈들임
        rollTween.Pause();
        
        while (GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down).matrixObject !=
               null)
        {
            
            yield return null;
        }
        
        moveTween.Play();
        rollTween.Play();
    }

    // FSM에서 "이동 끝났나?" 체크용 (다음 상태 전환 조건)
    public bool IsMoveFinished()
    {
        // 트윈 참조가 없거나, 죽었거나, 이미 완료(Complete)되었으면 '끝난 상태'로 판단
        if (moveTween == null || !moveTween.IsActive()) return true;
    
        return moveTween.IsComplete();
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
        mo.Animator.SetBool(Teleport, true);
        state = MoveState.Moving;
        
        yield return new WaitForSeconds(0.1f);
        
        transform.position = mo.GetCurrentCell().transform.position;
        
        yield return new WaitForSeconds(0.16f);

        state = MoveState.Staying;
        mo.Animator.SetBool(Teleport, false);
        
        startCell.state = MatrixCell.CellState.Empty;
        destCell.state = MatrixCell.CellState.Filled;
    }
}
