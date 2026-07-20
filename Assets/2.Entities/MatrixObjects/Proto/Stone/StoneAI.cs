using System;
using UnityEngine;

public class StoneAI : MonoBehaviour, IGridComponent
{
    private MatrixObject mo;
    GridMovement gridMovement;
    private GridGravity gravity;
    private GridRollable rollable;
    private GridPushable pushable;
    
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
        gravity = GetComponent<GridGravity>();
        rollable = GetComponent<GridRollable>();
        pushable = GetComponent<GridPushable>();
        
        mo.AppendGridComponent(this);
        mo.AppendGridComponent(pushable);
    }

    public void GridUpdate()
    {
        if (gridMovement.State == GridMovement.MoveState.Staying)
        {
            if (gravity.CanProcess())  
                gravity.Process();  
            else if (rollable.CanRoll())
                rollable.ExecuteRoll();
        }
    }
}
