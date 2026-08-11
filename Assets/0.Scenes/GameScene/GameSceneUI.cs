using System;
using TMPro;
using UnityEngine;

public class GameSceneUI : MonoBehaviour
{
    [SerializeField] GameSessionSO gameSession;
    [SerializeField] private TMP_Text levelNumText;
    [SerializeField] private TMP_Text levelNameText;
    
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text requiredChickenText;
    [SerializeField] private TMP_Text mushroomText;
    

    private int lastDisplayedSecond = -1;
    public void UpdateTimer(float playTime)
    {
        int currentSecond = Mathf.FloorToInt(playTime);
    
        // 초 단위가 바뀌었을 때만 UI Text 갱신 (GC Alloc 최적화)
        if (currentSecond != lastDisplayedSecond)
        {
            lastDisplayedSecond = currentSecond;
            TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Max(0f, playTime));
            timerText.text = timeSpan.ToString(@"hh\:mm\:ss");
        }
    }

    public void ShowChickenCount(int instanceRequiredChickenCount)
    {
        requiredChickenText.text = instanceRequiredChickenCount.ToString();
        if (instanceRequiredChickenCount == 0) requiredChickenText.color = Color.red;
    }

    public void ShowMushroomCount(int playerMushroomCount)
    {
        mushroomText.text = playerMushroomCount.ToString();
        if (playerMushroomCount == 0) mushroomText.color = Color.red;
        else mushroomText.color = Color.white;
    }

    public void ShowLevelInfo(LevelSaveData instanceLoadedLevelData)
    {
        levelNumText.text = gameSession.selectedOriginalLevelIndex.ToString();
        if(gameSession.isExploringOriginalLevel) levelNameText.text = gameSession.selectedOriginalLevelData.levelName;
        else levelNameText.text = instanceLoadedLevelData.levelName;
    }
}
