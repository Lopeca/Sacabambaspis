using TMPro;
using UnityEngine;

public class LevelInfoPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text levelStateText;
    
    [SerializeField] UserRuntimeDataSO userSaveData;

    public void ShowLevelInfo(LevelSaveData currentLoadedLevelData, int index)
    {
        //Debug.Log("ShowLevelInfo : " + index);
        levelNameText.text = currentLoadedLevelData.levelName;
        
        LevelState state = userSaveData.Data.GetLevelState(index);
        levelStateText.text = state.ToString();
        levelStateText.color = state.ToColor();
    }
}
