using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OriginalLevelSelectUIManager : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private OriginalLevelDatabase levelDB;
    [SerializeField] private GameSessionSO gameSessionSO;

    [Header("UI Panels")]
    [SerializeField] private LevelSelectPanel levelSelectPanel;
    [SerializeField] private LevelInfoPanel levelInfoPanel;
    [SerializeField] private LevelRecordPanel levelRecordPanel;
    
    // 비동기 작업 취소용 토큰 소스
    private CancellationTokenSource _selectCts;
    private void OnEnable()
    {
        LevelSelectButton.OnClickToFocus += FocusLevelButton;
        LevelSelectButton.OnClickToPlay += ExecuteLevelButton;
        
        gameSessionSO.isExploringOriginalLevel = true;
    }

    private void OnDisable()
    {
        LevelSelectButton.OnClickToFocus -= FocusLevelButton;
        LevelSelectButton.OnClickToPlay -= ExecuteLevelButton;
        
        
        gameSessionSO.isExploringOriginalLevel = false;
    }

    private void Start()
    {
        levelSelectPanel.Init(levelDB);
        
    }

    void FocusLevelButton(int index)
    {
        levelSelectPanel.FocusButton(index);
        
        
        // 1. 이전 진행 중이던 0.1초 대기 및 로드 작업을 "진짜로" 즉시 끊어버림
        _selectCts?.Cancel();
        _selectCts?.Dispose();
        _selectCts = new CancellationTokenSource();
       
        gameSessionSO.selectedOriginalLevelData = levelSelectPanel.CurrentSelectedButton.GetLevelData();
        gameSessionSO.selectedOriginalLevelIndex = index;
        
        // 2. 비동기 작업 시작
        SelectStageRoutineAsync(_selectCts.Token).Forget();

        
    }

    private async UniTaskVoid SelectStageRoutineAsync(CancellationToken token)
    {
        // [핵심] 시작하자마자 스크롤 취소 토큰(token) + GameObject 파괴 토큰을 결합!
        // 이렇게 만들어둔 linkedCts는 나중에 Dispose 해주는 것이 정석입니다.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            token, 
            this.GetCancellationTokenOnDestroy()
        );
    
        var linkedToken = linkedCts.Token;

        try
        {
            // [단계 1] 0.1초 숨고르기
            // 이제 0.01초 만에 오브젝트가 Destroy 되어도 즉시 Cancel 예외를 던지고 정지합니다.
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: linkedToken);

            // [단계 2] SO에 선택된 레벨 정보 세팅
            //gameSessionSO.selectedOriginalLevelData = levelData;

            // [단계 3] 어드레서블 로드 실행
            bool isSuccess = await gameSessionSO.LoadSelectedLevelAsync(linkedToken);

            // [단계 4] 로드 완료 후 UI 갱신
            if (isSuccess && gameSessionSO.CurrentLoadedLevelData != null)
            {
                levelInfoPanel.ShowLevelInfo(gameSessionSO.CurrentLoadedLevelData, levelSelectPanel.CurrentSelectedButton.Index);
                // TODO: 기록 패널에 기록 보여주기
                levelRecordPanel.ShowRecords(gameSessionSO.selectedOriginalLevelData);
            }
        }
        catch (OperationCanceledException)
        {
            // 스크롤을 넘겼거나, 0.1초 만에 오브젝트가 파괴되어 취소된 경우 모두 이쪽으로 안전하게 진입
        }
    }
    

    void ExecuteLevelButton(OriginalLevelData levelData)
    {
        gameSessionSO.selectedOriginalLevelData = levelData;
        SceneManager.LoadScene("GameScene");
    }
}
