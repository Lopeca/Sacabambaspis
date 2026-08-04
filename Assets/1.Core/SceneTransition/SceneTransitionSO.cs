using System.Collections;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneTransitionSO", menuName = "Scriptable Objects/SceneTransitionSO")]
public class SceneTransitionSO : ScriptableObject
{
    [SerializeField] private GameObject transitionPrefab;
    
    // NonSerialized를 붙여주면 에디터가 꺼져도 런타임 캐시가 직렬화되어 남는 것을 방지합니다.
    [System.NonSerialized] 
    private SceneTransitionManager runtimeInstance;
    public SceneTransitionManager RuntimeInstance => runtimeInstance;

    private void OnEnable()
    {
        // 에디터 실행/종료 시 메모리 캐시 깔끔하게 비우기
        runtimeInstance = null; 
    }

    public void EnsureWarmup()
    {
        if (runtimeInstance == null)
        {
            GameObject go = Instantiate(transitionPrefab);
            runtimeInstance = go.GetComponent<SceneTransitionManager>();
        }
    }
    
    public void LoadSceneWithFade(string sceneName)
    {
        // 1. null 체크 (OnEnable 덕분에 에디터 재시작 시에도 안전함)
        if (runtimeInstance == null)
        {
            Debug.Log("씬 트랜지션 SO Warm Up 누락 확인하기");
            GameObject go = Instantiate(transitionPrefab);
            runtimeInstance = go.GetComponent<SceneTransitionManager>();
        }

        // 2. 매니저(일꾼)에게 연출 명령
        runtimeInstance.StartFadeOutAndLoad(sceneName);
    }

    public void EnsureDark()
    {
        runtimeInstance.EnsureDark();
    }

    public IEnumerator FadeIn()
    {
        yield return runtimeInstance.FadeIn();
    }
}