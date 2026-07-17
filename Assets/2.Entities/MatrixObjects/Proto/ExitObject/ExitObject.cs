using System;
using UnityEngine;

public class ExitObject : MonoBehaviour, IGridObstacle
{
    public static event Action OnTryExit;
    
    public bool TryInteract(PlayerController player, Vector2Int direction)
    {
        OnTryExit?.Invoke();
        return false;
    }
}
