using System;
using UnityEngine;

public class ExitObject : MonoBehaviour, IGridObstacle
{
    public static event Action OnTryExit;
    
    public bool TryPassThrough(PlayerController player, Vector2Int direction)
    {
        OnTryExit?.Invoke();
        return false;
    }
}
