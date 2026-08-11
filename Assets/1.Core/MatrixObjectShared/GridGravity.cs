using System;
using UnityEngine;

[RequireComponent(typeof(GridMovement))]
public class GridGravity : MonoBehaviour, IGridComponent
{
    GridMovement gridMovement;
    ExplodeOnDeath explodeOnDeath;
    private MatrixObject mo;

    [SerializeField] private bool isFalling;
    [SerializeField] private bool isSensitive;
    public bool IsFalling => isFalling;

    private MatrixObject belowObject;

    public event Action OnStartFalling;
    public event Action OnEndFalling;
    

    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
        explodeOnDeath = GetComponent<ExplodeOnDeath>();
        isFalling = false;
    }
    

    public bool CanProcess()
    {
        if (gridMovement.State != GridMovement.MoveState.Staying) return false;

        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down);
        if (targetCell == null) return false;

        belowObject = GetBelowAttackableObject();
        bool canProcess = false;

        // 1. 아래칸이 완전히 비어있는 경우
        if (targetCell.state == MatrixCell.CellState.Empty) 
        {
            canProcess = true;
        }
        // 2. 이미 낙하 중이고, 아래에 짓눌릴 수 있는 대상(양배추 등)이 들어온 경우 (셀 상태 불문)
        else if (isFalling && belowObject != null && belowObject.isVulnerableToFalling) 
        {
            canProcess = true;
        }

        // 낙하 불가 판정 시 낙하 상태 정지
        if (!canProcess)
        {
            if (isFalling)
            {
                if (!isSensitive) 
                {
                    SoundManager.Instance.PlayGameSFX(SoundManager.Instance.landingSound, transform.position);
                    OnEndFalling?.Invoke();
                }
                else
                {
                    if (targetCell.state == MatrixCell.CellState.Filled)
                    {
                        explodeOnDeath.Explode();
                    }
                }
            }

            isFalling = false;
        }

        return canProcess;
    }

    public void Process()
    {
        isFalling = true;

        if (belowObject != null && belowObject.isVulnerableToFalling)
        {
            MatrixCell bottomCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down);

            // 아래 오브젝트가 플레이어가 아닌 경우 (양배추 등)
            if (GamePlayGridManager.Instance.player == null || belowObject != GamePlayGridManager.Instance.player.MO)
            {
                // 이동/공격 시도 중인 유닛이면 이동 트윈 취소 및 위치 정돈
                if (bottomCell.state == MatrixCell.CellState.Moving)
                {
                    belowObject.ForceCancelTween();
                }
                else if (bottomCell.state == MatrixCell.CellState.Attacking)
                {
                    belowObject.ForceCompleteTween();
                }

                // 피격 유닛 폭발
                belowObject.ExplodeOnDeath.Explode();  
            }
            // 아래 오브젝트가 플레이어인 경우
            else if (belowObject == GamePlayGridManager.Instance.player.MO)
            {
                // 플레이어가 가만히 있을 때만 덮쳐서 터뜨림
                if (bottomCell.state == MatrixCell.CellState.Filled)
                {
                    belowObject.ExplodeOnDeath.Explode();
                }
            }
        }
        else
        {
            OnStartFalling?.Invoke();
            gridMovement.ExecuteMove(Vector2Int.down, GridMovement.MoveState.Falling, MatrixCell.CellState.Falling);
        }
    }

    public void GridUpdate()
    {
        if (gridMovement.State != GridMovement.MoveState.Staying) return;

        // 아래칸이 비어있으면 다음 턴 낙하 후보로 등록
        if (GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down).state == MatrixCell.CellState.Empty)
            isFalling = true;
    }

    public MatrixObject GetBelowAttackableObject()
    {
        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down);
        if (targetCell == null) return null;

        MatrixObject _belowObject = null;

        // 1. 이미 칸에 차있는 경우
        if (targetCell.state == MatrixCell.CellState.Filled)
        {
            _belowObject = targetCell.matrixObject;
        }
        // 2. 다른 방향에서 기어들어오는 중인 경우 (Moving / Attacking)
        else if (targetCell.state == MatrixCell.CellState.Moving || targetCell.state == MatrixCell.CellState.Attacking)
        {
            // 위에서 아래로 떨어지는 게 아닌, 옆/밑에서 진입하는 객체 감지
            if (targetCell.moveStateDirection != Vector2Int.down)
            {
                _belowObject = targetCell.matrixObject != null ? targetCell.matrixObject : targetCell.GetMovingObject();
            }
        }

        return _belowObject;
    }
}