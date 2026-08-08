using System;
using UnityEngine;

public class TabiCompass : MonoBehaviour, IGridInteractable
{
    public static event Action OnCompassInteract;
    
    bool hasInteracted;

    private MatrixObject mo;
    [SerializeField] Sprite deactivatedSprite;
    [SerializeField] AudioClip deactivationSound;
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        hasInteracted = false;
    }

    private void OnEnable()
    {
        OnCompassInteract += ShutDown;
    }

    public void Interact(PlayerController player, Vector2Int direction)
    {
        if (!hasInteracted)
        {
            OnCompassInteract?.Invoke();
        }
    }

    void ShutDown()
    {
        hasInteracted = true;
        SoundManager.Instance.PlayGameSFX(deactivationSound, transform.position);
        if (deactivatedSprite != null && mo != null && mo.SpriteRenderer != null)
        {
            mo.SpriteRenderer.sprite = deactivatedSprite;
        }
        OnCompassInteract -= ShutDown;
    }

    private void OnDisable()
    {
        OnCompassInteract -= ShutDown;

    }

    public bool Continuous { get; set; }
}
