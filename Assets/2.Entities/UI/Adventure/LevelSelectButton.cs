using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] UserRuntimeDataSO userDataSo;
    [SerializeField] private LevelState buttonState;
    [SerializeField] OriginalLevelData originalLevelData;
    [SerializeField] private int index;
    public int Index => index;
    [SerializeField] private TMP_Text levelIndexText;
    [SerializeField] private Image levelIndexFrame;
    [SerializeField] TMP_Text levelName;
    [SerializeField] private Image background;

    readonly Color selectedColor = Color.cyan;

    private bool isSelected;

    public static event Action<int> OnClickToFocus;
    public static event Action<OriginalLevelData> OnClickToPlay;

    public void Init(int index, OriginalLevelData originalLevelData)
    {
        this.index = index;
        this.originalLevelData = originalLevelData;
        levelIndexText.text = index.ToString("D3");
        this.levelName.text = originalLevelData.levelName;
        buttonState = userDataSo.Data.GetLevelState(index);
        
        InitColor();
    }

    public void Refresh()
    {
        buttonState = userDataSo.Data.GetLevelState(index);
        
        InitColor();
    }

    private void InitColor()
    {
        Color targetColor = buttonState.ToColor();
        
        levelIndexFrame.color = targetColor;
        levelName.color = targetColor;
        levelIndexText.color = targetColor;
    }

    public void OnClick()
    {
        if (!isSelected)
        {
            Select();
            OnClickToFocus?.Invoke(index);
        }
        else
        {
            if(buttonState != LevelState.Locked)
                OnClickToPlay?.Invoke(originalLevelData);
        }
    }

    public void Select()
    {
        isSelected = true;
            
        levelIndexText.color = selectedColor;
        levelName.color = selectedColor;
        background.color = new Color(background.color.r, background.color.g, background.color.b, 0.2f);
    }

    public void DeSelect()
    {
        isSelected = false;
        
        background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
        
        InitColor();
    }

    public OriginalLevelData GetLevelData()
    {
        return originalLevelData;
    }
}
