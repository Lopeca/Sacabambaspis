using System;
using System.Collections;
using _1.Core;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    private MatrixObject mo;
    public MatrixObject MO => mo;


    public AudioClip installSound;
    ExplodeOnDeath explodeComponent;

    public void Init()
    {
        mo = GetComponent<MatrixObject>();
    }

    private void Start()
    {
        explodeComponent = mo.ExplodeOnDeath;
        SoundManager.Instance.PlayGameSFX(installSound, transform.position, 0.7f);
    }

    private void OnEnable()
    {
        StartCoroutine(MushroomCoroutine());
    }

    private IEnumerator MushroomCoroutine()
    {
        yield return new WaitForSeconds(GameConstants.MUSHROOM_FUSE_TIME);
        explodeComponent.Explode();
        Destroy(gameObject);
    }
}
