using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GridRollable : MonoBehaviour
{
    private GridMovement gridMovement;
    private MatrixObject mo;
    private Vector2Int rollDirection;

    [SerializeField] private MatrixObject aboveObject;  // 디버그 추적용

    private void Awake()
    {
        gridMovement = GetComponent<GridMovement>();
        mo = GetComponent<MatrixObject>();
    }

    public bool CanRoll()
    {
        if (GetCell(mo.GetPos() + Vector2Int.down).state != MatrixCell.CellState.Filled)
            return false;
        
        if (GetCell(mo.GetPos() + Vector2Int.down).matrixObject.isRounded == false)
            return false;
        
        
        if (GetCell(mo.GetPos() + Vector2Int.left).state == MatrixCell.CellState.Empty
            && GetCell(mo.GetPos() + Vector2Int.left + Vector2Int.down).matrixObject == null)
        {
            rollDirection = Vector2Int.left;
            
            aboveObject = GetCell(mo.GetPos()+rollDirection+Vector2Int.up)?.matrixObject;
            if (aboveObject != null && aboveObject.GridGravity != null) return false;
            return true;
        }
        if (GetCell(mo.GetPos() + Vector2Int.right).state == MatrixCell.CellState.Empty
                 && GetCell(mo.GetPos() + Vector2Int.right + Vector2Int.down).matrixObject == null)
        {
            rollDirection = Vector2Int.right;
            
            aboveObject = GetCell(mo.GetPos()+rollDirection+Vector2Int.up)?.matrixObject;
            if (aboveObject != null && aboveObject.GridGravity != null) return false;
            return true;
        }

        return false;
    }

    public void ExecuteRoll()
    {
        gridMovement.ExecuteMove(rollDirection, GridMovement.MoveState.Rolling, MatrixCell.CellState.Attacking,true);
        gridMovement.ExecuteRoll(rollDirection);
    }

    MatrixCell GetCell(Vector2Int pos)
    {
        return GamePlayGridManager.Instance.GetCell(pos);
    }
    

}
