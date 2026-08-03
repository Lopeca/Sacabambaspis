using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] private GameSessionSO gameSessionSO;
    [SerializeField] private SceneTransitionSO sceneTransitionSO;

    private Coroutine initializeSequenceCoroutine;

    [SerializeField] OriginalLevelData demoLevelData;
    // [SerializeField] private AssetReference demoStageAddress;

    private void Awake()
    {
        sceneTransitionSO.EnsureWarmup();
    }

    // async UniTaskVoid 대신 async UniTask 사용
    private async UniTask Start()
    {
        Debug.Log("GameSceneManager: Start");
        // 1. 이미 레벨 데이터가 세팅되어 있는 경우 (정상적인 레벨 선택 씬 진입)
        if (gameSessionSO.selectedOriginalLevelData != null)
        {
            Debug.Log("레벨데이터 감지");
            initializeSequenceCoroutine = StartCoroutine(InitializeSequence());
            return;
        }

        // 2. 레벨 선택 씬을 안 거치고 현재 씬에서 바로 Play를 눌렀을 때 (에디터 테스트용 데모 스테이지)
        try
        {
            if (demoLevelData.levelAddress != null && demoLevelData.levelAddress.RuntimeKeyIsValid())
            {
                gameSessionSO.selectedOriginalLevelData = demoLevelData;
            }
            else
            {
                Debug.LogError("[GameSceneManager] 데모 스테이지 Address가 설정되지 않았습니다.");
                return;
            }

            var destroyToken = this.GetCancellationTokenOnDestroy();

            bool isSuccess = await gameSessionSO.LoadSelectedLevelAsync(destroyToken);

            if (isSuccess && gameSessionSO.CurrentLoadedLevelData != null)
            {
                initializeSequenceCoroutine = StartCoroutine(InitializeSequence());
            }
            else
            {
                Debug.LogError("[GameSceneManager] 데모 스테이지 로드 실패 또는 취소됨.");
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[GameSceneManager] 데모 스테이지 로드가 취소되었습니다.");
        }
    }

    private IEnumerator InitializeSequence()
    {
        sceneTransitionSO.EnsureDark();
        GamePlayGridManager.Instance.InstantiateOriginalLevel(gameSessionSO.CurrentLoadedLevelData);

        yield return sceneTransitionSO.FadeIn();

        // TODO: 게임 시뮬레이션 시작
    }
}