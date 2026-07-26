using System;
using UnityEngine;

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
        belowObject = GetBelowAttackableObject();
        bool canProcess;

        if (targetCell.state == MatrixCell.CellState.Empty) canProcess = true; // 아래칸이 비어있으면
        else if (isFalling && belowObject != null && belowObject.isVulnerableToFalling) canProcess = true;
        else canProcess = false;

        if (!canProcess)
        {
            if (isFalling)
            {
                if (!isSensitive) OnEndFalling?.Invoke(); // 아직 isSensitive가 false면서 이 이벤트에 구독을 거는 오브젝트는 없어서 지워도 무방함
                else
                {
                    if (!targetCell.HasPlayer() || targetCell.state == MatrixCell.CellState.Filled)
                    {
                        Debug.Log("리제");
                        explodeOnDeath.Explode(); // 일단은 약한 물체는 낙하 후 터지는 걸 전제함
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

            // 아래 오브젝트가 플레이어가 아닐 때
            if (belowObject != GamePlayGridManager.Instance.player.MO)
            {
                // 이동중인 오브젝트면 트윈 강제 종료 후 원위치로 끌어당기기
                if (bottomCell.state ==
                    MatrixCell.CellState.Moving)
                {
                    belowObject.ForceCompleteTween();
                    belowObject.MoveToTargetCell(GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down));
                }

                belowObject.ExplodeOnDeath.Explode();
            }
            else if (belowObject == GamePlayGridManager.Instance.player.MO &&
                     bottomCell.state == MatrixCell.CellState.Filled) // 플레이어면 안움직이면 터뜨림
            {
                belowObject.ExplodeOnDeath.Explode();
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

        if (GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down).state == MatrixCell.CellState.Empty)
            isFalling = true;
    }

    public MatrixObject GetBelowAttackableObject()
    {
        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down);
        MatrixObject _belowObject;

        if (targetCell.state == MatrixCell.CellState.Filled)
            _belowObject = targetCell.matrixObject;
        else if (targetCell.state == MatrixCell.CellState.Moving && targetCell.moveStateDirection != Vector2Int.down)
            _belowObject = targetCell.GetMovingObject();
        else return null;

        return _belowObject;
    }
}