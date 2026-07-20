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
        belowObject = targetCell.matrixObject;
        bool canProcess; 
        
        if(targetCell.state == MatrixCell.CellState.Empty) canProcess = true;
        else if (isFalling && targetCell.matrixObject.isVulnerableToFalling) canProcess = true;
        else canProcess = false;
        
        return canProcess;
    }

    public void Process()
    {
        isFalling = true;

        if (belowObject != null && belowObject.isVulnerableToFalling)
        {
            belowObject.ExplodeOnDeath.Explode();
        }
        else
        {
            isFalling = false;
            gridMovement.ExecuteMove(Vector2Int.down, GridMovement.MoveState.Falling, MatrixCell.CellState.Falling);
        }
    }

    public void GridUpdate()
    {
        if (gridMovement.State != GridMovement.MoveState.Staying) return;
        
        if (GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down).state == MatrixCell.CellState.Empty)
            isFalling = true;
    }
}
