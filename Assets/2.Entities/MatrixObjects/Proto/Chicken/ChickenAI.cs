using UnityEngine;

public class ChickenAI : MonoBehaviour, IGridComponent
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
