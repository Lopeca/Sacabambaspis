using System;
using UnityEngine;

public class RemoteBomb : MonoBehaviour
{
    ExplodeOnDeath explodeOnDeath;

    private void Awake()
    {
        explodeOnDeath = GetComponent<ExplodeOnDeath>();
    }

    private void OnEnable()
    {
        TabiCompass.OnCompassInteract += Explode;
    }

    private void OnDisable()
    {
        TabiCompass.OnCompassInteract -= Explode;
    }

    private void Explode()
    {
        explodeOnDeath.Explode();
    }
}
