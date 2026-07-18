using UnityEngine;

public class GridGravity : MonoBehaviour
{
    GridMovement gridMovement;
    private MatrixObject mo;
    
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
    }
    
    public bool CanFall()
    {
        return GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down).state == MatrixCell.CellState.Empty;
    }

    public void ExecuteFall()
    {
        gridMovement.ExecuteMove(Vector2Int.down, GridMovement.MoveState.Moving, true);
    }
}
