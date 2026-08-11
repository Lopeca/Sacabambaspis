using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;
    
    [Header("SO")]
    [SerializeField] private GameSessionSO gameSessionSO;
    [SerializeField] private SceneTransitionSO sceneTransitionSO;
    [SerializeField] private UserRuntimeDataSO userRuntimeDataSO;
    
    [Header("참조")]
    [SerializeField] GameCamera gameCamera;
    [SerializeField] GameSceneUI gameSceneUI;
    [SerializeField] AudioClip helloSound;

    [Header("Collect Effects")]
    [SerializeField]  ChickenCollectEffect chickenCollectEffect;
    [SerializeField]  MushroomCollectEffect mushroomCollectEffect;
    private Coroutine initializeSequenceCoroutine;
    [Header("DemoData")]
    [SerializeField] OriginalLevelData demoLevelData;
    // [SerializeField] private AssetReference demoStageAddress;

    private float playTime;

    private bool isPlaying;
    private bool isDemo;
    private void Awake()
    {
        Instance = this;
        
        sceneTransitionSO.EnsureWarmup();
        playTime = 0;
        isPlaying = false;
    }

    private void OnEnable()
    {
        chickenCollectEffect.OnCollected += ShowChickenCount;
        mushroomCollectEffect.OnCollected += ShowMushroomCount;
        GamePlayGridManager.Instance.OnPlayerUsedMushroom += ShowMushroomCount;
        GamePlayGridManager.Instance.OnGameOver += HandleResult;
    }

    private void OnDisable()
    {
        chickenCollectEffect.OnCollected -= ShowChickenCount;
        mushroomCollectEffect.OnCollected -= ShowMushroomCount;
        GamePlayGridManager.Instance.OnPlayerUsedMushroom -= ShowMushroomCount;
        GamePlayGridManager.Instance.OnGameOver -= HandleResult;
        

    }


    // async UniTaskVoid 대신 async UniTask 사용

    // ReSharper disable Unity.IncorrectMethodSignature

    private async UniTask Start()
    {
        Debug.Log("GameSceneManager: Start");
        // 1. 이미 레벨 데이터가 세팅되어 있는 경우 (정상적인 레벨 선택 씬 진입)
        if (gameSessionSO.selectedOriginalLevelData != null && gameSessionSO.selectedOriginalLevelIndex != -1)
        {
            Debug.Log("레벨데이터 감지");
            initializeSequenceCoroutine = StartCoroutine(InitializeSequence());
            return;
        }

        // 2. 레벨 선택 씬을 안 거치고 현재 씬에서 바로 Play를 눌렀을 때 (에디터 테스트용 데모 스테이지)
        try
        {
            isDemo = true;
            if (demoLevelData.levelAddress != null && demoLevelData.levelAddress.RuntimeKeyIsValid())
            {
                gameSessionSO.selectedOriginalLevelData = demoLevelData;
                gameSessionSO.isExploringOriginalLevel = true;
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

    private void FixedUpdate()
    {
        if (isPlaying)
            UpdatePlayTime();
    }

    private void UpdatePlayTime()
    {
        playTime += Time.fixedDeltaTime;
        gameSceneUI.UpdateTimer(playTime);
    }

    private IEnumerator InitializeSequence()
    {
        sceneTransitionSO.EnsureDark();
        GamePlayGridManager.Instance.InstantiateOriginalLevel(gameSessionSO.CurrentLoadedLevelData);
        InitializeUI();
        
        gameCamera.SetTarget(GamePlayGridManager.Instance.player.transform);
        yield return sceneTransitionSO.FadeIn();
        
        SoundManager.Instance.PlayGlobalGameSFX(helloSound);
        isPlaying = true;
        GamePlayGridManager.Instance.StartPlaying();
        
        
    }

    private void InitializeUI()
    {
        ShowLevelInfo();
        ShowChickenCount();
        ShowMushroomCount();
    }

    private void ShowLevelInfo()
    {
        gameSceneUI.ShowLevelInfo(GamePlayGridManager.Instance.LoadedLevelData);
    }

    void ShowChickenCount()
    {
        gameSceneUI.ShowChickenCount(GamePlayGridManager.Instance.RequiredChickenCount);
    }

    private void ShowMushroomCount()
    {
        if (GamePlayGridManager.Instance.player == null) return;
        gameSceneUI.ShowMushroomCount(GamePlayGridManager.Instance.player.MushroomCount);
    }

    private void HandleResult()
    {
        isPlaying = false;
        GamePlayGridManager.Instance.isPlaying = false;
        
        if (isDemo)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        // if (!GamePlayGridManager.Instance.isCleared) return;
        // 기록 저장
        if (gameSessionSO.isExploringOriginalLevel)
        {
            if (GamePlayGridManager.Instance.isCleared)
            {
                userRuntimeDataSO.UnlockOriginalStage(gameSessionSO.selectedOriginalLevelIndex);
                userRuntimeDataSO.AddRecord(gameSessionSO.selectedOriginalLevelData.LevelID, playTime);
                userRuntimeDataSO.Save();
            }
        }
        // TODO : 유저 커스텀맵일 경우 AddRecord 할 때 MD5 Hash 방식 고려하기(+오리지널 데이터도 이 방식을 써도 되지만 일단 코드를 다 짜서 패스)
        
        // 진행도 저장

        gameSessionSO.gameSceneEnded = true;
        sceneTransitionSO.LoadSceneWithFade("AdventureScene");
    }
}