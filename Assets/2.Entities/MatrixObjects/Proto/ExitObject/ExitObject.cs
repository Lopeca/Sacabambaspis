using System;
using UnityEngine;

public class ExitObject : MonoBehaviour, IGridInteractable
{
    public static event Action OnTryExit;
    public bool Continuous { get; set; }
    
    public void Interact(PlayerController player, Vector2Int direction)
    {
        OnTryExit?.Invoke();
    }

}
