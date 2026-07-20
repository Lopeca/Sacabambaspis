using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 이동관련 유틸을 제공하는 컴포넌트, GridUpdate를 필요로하는 코드가 아닌 여타 오브젝트 이동 관련 컴포넌트의 이동로직 보조형
/// </summary>
public class GridMovement : MonoBehaviour
{
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
    public ObjectPhysicsConfigSO physics;

    private Tween moveTween;
    Tween rollTween;
    
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
        // 1. 오브젝트가 가만히 있지 않고 뭔가 동작 중이면 이 함수가 실행될 이유가 없음
        if (state != MoveState.Staying)
        {
            Debug.Log("움직이는 오브젝트에 이동명령이 실행됨. 이동 검사 로직 확인 필요");
            Debug.Break();
            return;
        }
        
        startPos = new Vector2Int(mo.posX, mo.posY);
        destPos = new Vector2Int(mo.posX + direction.x, mo.posY + direction.y);
        Vector3 destWorldPos = GamePlayGridManager.Instance.GetCell(destPos).transform.position;
        
        state = targetState;
        
        // 2. 이동에 관여되는 셀들을 잠그고 로직상 이동은 미리 완료함
        GamePlayGridManager.Instance.SetCellState(startPos, MatrixCell.CellState.Moving);
        GamePlayGridManager.Instance.SetCellState(destPos, destState);
        GamePlayGridManager.Instance.MoveMatrixObjectPosition(mo, direction);
        
        //GamePlayGridManager.Instance.ReserveMove(startPos, destPos, isAttack);
        // 3. 이동이 완료될 때까지 
        PerformMove(destWorldPos, CompleteMove, isPlayerSpeed);
    }
    
    /// <summary>
    /// Pusher 컴포넌트에 필요하여 분리된 포지션 이동 트윈과 트윈중의 오브젝트 상태 관리만을 담당하는 함수
    /// </summary>
    /// <param name="destPos"></param>
    /// <param name="onComplete"></param>
    /// <param name="isPlayerSpeed"></param>
    public void PerformMove(Vector3 destPos, Action onComplete = null, bool isPlayerSpeed = false)
    {
        state = MoveState.Moving;
        moveTween =transform.DOMove(destPos, isPlayerSpeed ? GamePlayGridManager.Instance.playerConfigSO.moveDuration : physics.moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                onComplete?.Invoke();

                state = MoveState.Staying;
            });
    }

    void CompleteMove()
    {
        GamePlayGridManager.Instance.SetCellState(startPos, MatrixCell.CellState.Empty);
        GamePlayGridManager.Instance.SetCellState(destPos, MatrixCell.CellState.Filled);
    }

    public void ForceCompleteMove()
    {
        // 트윈의 OnComplete에 액션으로 CompleteMove 혹은 CompletePush를 걸었기 때문에 트윈만을 강제로 끝냄으로써 이동을 강제 완료함
        moveTween.Complete();
        rollTween.Complete();
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
    public void ExecuteRollTween(Vector2Int direction, bool isPlayerSpeed = false)
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
    }
}
