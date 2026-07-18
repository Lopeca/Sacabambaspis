using System;
using DG.Tweening;
using UnityEngine;

public class GridRollable : MonoBehaviour
{
    private GridMovement gridMovement;
    private MatrixObject mo;
    private Vector2Int rollDirection;


    private void Awake()
    {
        gridMovement = GetComponent<GridMovement>();
        mo = GetComponent<MatrixObject>();
    }

    public bool CanRoll()
    {
        if (GetCell(mo.GetPos() + Vector2Int.down).state == MatrixCell.CellState.Filled
            && GetCell(mo.GetPos() + Vector2Int.down).matrixObject.isRounded == false)
            return false;
        
        if (GetCell(mo.GetPos() + Vector2Int.left).state == MatrixCell.CellState.Empty
            && GetCell(mo.GetPos() + Vector2Int.left + Vector2Int.down).state == MatrixCell.CellState.Empty)
        {
            rollDirection = Vector2Int.left;
            return true;
        }
        else if (GetCell(mo.GetPos() + Vector2Int.right).state == MatrixCell.CellState.Empty
                 && GetCell(mo.GetPos() + Vector2Int.right + Vector2Int.down).state == MatrixCell.CellState.Empty)
        {
            rollDirection = Vector2Int.right;
            return true;
        }

        return false;
    }

    public void ExecuteRoll()
    {
        gridMovement.ExecuteMove(rollDirection, GridMovement.MoveState.Moving, true);
        gridMovement.ExecuteRollTween(rollDirection);
    }
    
    

    MatrixCell GetCell(Vector2Int pos)
    {
        return GamePlayGridManager.Instance.GetCell(pos);
    }
    

}
