using UnityEngine;

public class GridGravity : MonoBehaviour, IGridComponent
{
    GridMovement gridMovement;
    private MatrixObject mo;

    [SerializeField]private bool isFalling;
    public bool IsFalling => isFalling;

    private MatrixObject belowObject;
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
        isFalling = false;
    }
    
    public bool CanProcess()
    {
        if (gridMovement.State != GridMovement.MoveState.Staying) return false;
        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down);
        belowObject = GetBelowAttackableObject();
        bool canProcess; 
        
        if(targetCell.state == MatrixCell.CellState.Empty) canProcess = true;   // 아래칸이 비어있으면
        else if (belowObject != null && belowObject.isVulnerableToFalling) canProcess = true;
        else canProcess = false;

        if (!canProcess) isFalling = false;
        return canProcess;
    }

    public void Process()
    {
        isFalling = true;

        if (belowObject != null && belowObject.isVulnerableToFalling)
        {
            // 아래 오브젝트가 플레이어가 아니거나, 플레이어지만 제자리일 때
            if (belowObject != GamePlayGridManager.Instance.player.MO)
            {
                // 아래칸이 비어있으면 이동중인 오브젝트인것. 아니면 냅두고 그냥 폭발
                if (GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down).state ==
                    MatrixCell.CellState.Empty)
                {
                    belowObject.ForceCompleteTween();
                    belowObject.MoveToTargetCell(GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down));
                }

                belowObject.ExplodeOnDeath.Explode();
            }
            
        }
        else
        {
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
