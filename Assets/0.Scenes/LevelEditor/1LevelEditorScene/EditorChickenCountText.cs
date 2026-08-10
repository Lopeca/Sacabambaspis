using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class EditorChickenCountText : MonoBehaviour
{
   [SerializeField] ChickenCollectEffect chickenCollectEffect;
   [SerializeField] TMP_Text chickenCountText;
   
   Coroutine showChickenCountCoroutine;
   private void OnEnable()
   {
      chickenCollectEffect.OnCollected += ShowChickenCount;
      ShowChickenCount();
   }

   private void OnDisable()
   {
      chickenCollectEffect.OnCollected -= ShowChickenCount;
      if(showChickenCountCoroutine != null) StopCoroutine(showChickenCountCoroutine);
   }

   void ShowChickenCount()
   {
      if(showChickenCountCoroutine != null) StopCoroutine(showChickenCountCoroutine);
      showChickenCountCoroutine = StartCoroutine(ShowChickenCountAfterFrame());
   }

   IEnumerator ShowChickenCountAfterFrame()
   {
      yield return null;
      chickenCountText.text = GamePlayGridManager.Instance.RequiredChickenCount.ToString();
   }

   public void SetText(string text)
   {
      chickenCountText.text = text;
      Debug.Log(text);
   }
}
