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
    /// <param name="isAttack"> 플레이어가 이 오브젝트의 도착지로 진입 시도 시 게임 오버가 되도록 </param>
    /// <param name="isPlayerSpeed"></param>
    
    public void ExecuteMove(Vector2Int direction, MoveState targetState, bool isAttack = false, bool isPlayerSpeed = false)
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
        GamePlayGridManager.Instance.SetCellState(startPos, MatrixCell.CellState.Using);
        GamePlayGridManager.Instance.SetCellState(destPos, isAttack ? MatrixCell.CellState.Danger : MatrixCell.CellState.Using);
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
        transform.DOMove(destPos, isPlayerSpeed ? GamePlayGridManager.Instance.playerConfigSO.moveDuration : physics.moveDuration)
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

    public void ForceState(MoveState state)
    {
        this.state = state;
    }
}
