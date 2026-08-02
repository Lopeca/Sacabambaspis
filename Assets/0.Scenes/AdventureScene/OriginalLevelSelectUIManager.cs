using System;
using UnityEngine;

public class OriginalLevelSelectUIManager : MonoBehaviour
{
    
    [SerializeField] private OriginalLevelDatabase levelDB;
    [SerializeField] GameSessionSO gameSession;
    [SerializeField] LevelSelectPanel levelSelectPanel;
    
    private void OnEnable()
    {
        LevelSelectButton.OnClickToFocus += FocusLevelButton;
        
        gameSession.isExploringOriginalLevel = true;
    }

    private void OnDisable()
    {
        LevelSelectButton.OnClickToFocus -= FocusLevelButton;
        
        gameSession.isExploringOriginalLevel = false;
    }

    private void Start()
    {
        levelSelectPanel.Init(levelDB);
    }

    void FocusLevelButton(int index)
    {
        levelSelectPanel.FocusButton(index);
        // TODO:맵정보 패널에 정보 보여주기
        // 기록 패널에 기록 보여주기
    }

    void ExecuteLevelButton(OriginalLevelData levelData)
    {
        gameSession.selectedOriginalLevelData = levelData;
    }
}
