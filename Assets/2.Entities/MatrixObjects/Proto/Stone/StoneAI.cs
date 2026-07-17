using System;
using UnityEngine;

public class StoneAI : MonoBehaviour, IActiveGridElement
{
    private MatrixObject mo;
    GridMovement gridMovement;
    
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
    }

    public void GridUpdate()
    {
        if (gridMovement.State == GridMovement.MoveState.Staying)
        {
            if (CanFall())  // 
                ExecuteFall();  // 낙하 실패시 구르기 
        }
    }

    private bool CanFall()
    {
        Debug.Log(mo.GetPos() - Vector2Int.down);
        return GamePlayGridManager.Instance.GetCell(mo.GetPos() + Vector2Int.down).state == MatrixCell.CellState.Empty;
    }

    private void ExecuteFall()
    {
        gridMovement.ExecuteMove(Vector2Int.down, GridMovement.MoveState.Moving, true);
    }
}
