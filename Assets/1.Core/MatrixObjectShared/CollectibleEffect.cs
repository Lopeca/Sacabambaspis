using System;
using UnityEngine;

public abstract class CollectibleEffect : ScriptableObject
{
    public event Action OnCollected;

    public virtual void ApplyEffect()
    {
        OnCollected?.Invoke();
    }
}
