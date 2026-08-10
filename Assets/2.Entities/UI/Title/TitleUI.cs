using System;
using UnityEngine;

public class TitleUI : MonoBehaviour
{
    // 씬에 상주하고 씬 바뀌면 터지는 게 확실한 싱글톤이라서 복잡한 처리는 건너뜀
    public static TitleUI Instance;

    [SerializeField] TitleMainPanel mainPanel;
    [SerializeField] TitleStartPanel startPanel;
    [SerializeField] GameSettingSO gameSetting;
    [SerializeField] SceneTransitionSO sceneTransition;
    
    [SerializeField] GameObject manualPanel;
    private void Awake()
    {
        Instance = this;
        
        gameSetting.LoadSettings();
        gameSetting.EnsureUIWarmup();
    }

    private void Start()
    {
        ShowMainPanel();
        sceneTransition.EnsureWarmup();
        StartCoroutine(sceneTransition.FadeIn());
    }

    public void ShowMainPanel()
    {
        SetAllPanelsActive(false);
        mainPanel.gameObject.SetActive(true);
        manualPanel.SetActive(false);
    }

    public void ShowStartPanel()
    {
        SetAllPanelsActive(false);
        startPanel.gameObject.SetActive(true);
    }

    public void ShowSettingPanel()
    {
        gameSetting.RuntimeInstance.gameObject.SetActive(true);
    }

    private void SetAllPanelsActive(bool isActive)
    {
        mainPanel.gameObject.SetActive(isActive);
        startPanel.gameObject.SetActive(isActive);
    }

    public void OnClickQuitBtn()
    {
        Application.Quit();
    }

    public void OnClickManualBtn()
    {
        manualPanel.SetActive(true);
    }
    public void OnClickManualCloseBtn()
    {
        manualPanel.SetActive(false);
    }
}
