using System;
using UnityEngine;

public class StoneAI : MonoBehaviour, IGridComponent
{
    private MatrixObject mo;
    GridMovement gridMovement;
    private GridGravity gravity;
    private GridRollable rollable;
    
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
        gravity = GetComponent<GridGravity>();
        rollable = GetComponent<GridRollable>();
        
        
        mo.AppendGridComponent(this);

        gridMovement.AfterOnMoveCompleted += ObserveSurroundingAreaAndAct;
    }

    public void GridUpdate()
    {
        if (gridMovement.State == GridMovement.MoveState.Staying)
        {
            ObserveSurroundingAreaAndAct();
        }
    }

    private void ObserveSurroundingAreaAndAct()
    {
        // 중복 방지: Staying 상태가 아니면 실행하지 않음
        if (gridMovement.State != GridMovement.MoveState.Staying) return;
        
        if (gravity.CanProcess())  
            gravity.Process();  
        else if (rollable != null && rollable.CanRoll())
            rollable.ExecuteRoll();
    }

    private void OnDestroy()
    {
        gridMovement.AfterOnMoveCompleted -= ObserveSurroundingAreaAndAct;
    }
}
