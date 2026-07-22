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
        
        if(targetCell.state == MatrixCell.CellState.Empty) canProcess = true;   // 아래칸이 비어있으면
        else if (isFalling
                 && targetCell.state == MatrixCell.CellState.Filled             // 막 떨어지는 중이었고 아래칸이 이동 처리중이 아니고 떨어지는 물체에 피해를 입는 물체면
                 && targetCell.matrixObject.isVulnerableToFalling)
        {
            canProcess = true;
        }
        else canProcess = false;

        if (!canProcess) isFalling = false;
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
