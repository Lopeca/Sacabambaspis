using System;
using UnityEngine;

public class ExitObject : MonoBehaviour, IGridInteractable
{
    public static event Action OnTryExit;
    [SerializeField] AudioClip exitSound;
    public bool Continuous { get; set; }
    
    public void Interact(PlayerController player, Vector2Int direction)
    {
        SoundManager.Instance.PlayGlobalGameSFX(exitSound);
        OnTryExit?.Invoke();
    }

}
