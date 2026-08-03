using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] Image blackScreen;

    private const float duration = 0.5f;

    private Tween currentTween;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        blackScreen.color = blackScreen.color.WithAlpha(0);
        blackScreen.gameObject.SetActive(false);
    }

    public void EnsureDark()
    {
        blackScreen.color = blackScreen.color.WithAlpha(1);
        blackScreen.gameObject.SetActive(true);
    }

    public void StartFadeOutAndLoad(string sceneName)
    {
        blackScreen.gameObject.SetActive(true);
        
        blackScreen.DOColor(blackScreen.color.WithAlpha(1), duration).OnComplete(()=>SceneManager.LoadScene(sceneName));
    }

    public IEnumerator FadeIn()
    {
        blackScreen.color = blackScreen.color.WithAlpha(1);
        blackScreen.gameObject.SetActive(true);
        
        if(currentTween != null) currentTween.Kill();
        currentTween = blackScreen.DOColor(blackScreen.color.WithAlpha(0), duration);
        
        yield return currentTween.WaitForCompletion();
    }

    private void OnDisable()
    {
        if (currentTween != null)
            currentTween.Kill();
    }
}
