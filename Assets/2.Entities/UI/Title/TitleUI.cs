using System;
using UnityEngine;

public class TitleUI : MonoBehaviour
{
    // 씬에 상주하고 씬 바뀌면 터지는 게 확실한 싱글톤이라서 복잡한 처리는 건너뜀
    public static TitleUI Instance;

    [SerializeField] TitleMainPanel mainPanel;
    [SerializeField] TitleStartPanel startPanel;
    [SerializeField] SettingPanel settingPanel;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShowMainPanel();
    }

    public void ShowMainPanel()
    {
        SetAllPanelsActive(false);
        mainPanel.gameObject.SetActive(true);
    }

    public void ShowStartPanel()
    {
        SetAllPanelsActive(false);
        startPanel.gameObject.SetActive(true);
    }

    public void ShowSettingPanel()
    {
        SetAllPanelsActive(false);
        settingPanel.gameObject.SetActive(true);
        // setting 패널 액션 처리 필요
    }

    private void SetAllPanelsActive(bool isActive)
    {
        mainPanel.gameObject.SetActive(isActive);
        startPanel.gameObject.SetActive(isActive);
        settingPanel.gameObject.SetActive(isActive);
    }

    public void OnClickQuitBtn()
    {
        Application.Quit();
    }
}
