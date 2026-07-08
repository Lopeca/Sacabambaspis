using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelDesignHubManager : MonoBehaviour
{
    public static LevelDesignHubManager Instance;
    
    private CustomLevelFileSystem.LevelFolderNode node;

    private Stack<CustomLevelFileSystem.LevelFolderNode> nodeList;
    
    [SerializeField] GameObject contentRoot;
    [SerializeField] private GameObject navBackBtnPrefab;
    [SerializeField] private GameObject navFolderBtnPrefab;
    [SerializeField] private GameObject navLevelBtnPrefab;
    
    [SerializeField] private GameObject popupRayLockPanel;

    [Header("팝업")] 
    public GameObject createFolderPopup;


    // 레벨 작업을 하다가 돌아왔을 때 마지막 탐색하던 폴더를 계속 보기 위한 싱글톤 보존 플래그
    // 즉 편집 모드를 나갔을 때는 싱글톤 파괴로 폴더 탐색 현황이 초기화되도록대응할 예정
    bool initialized;
    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (initialized) return;
        
        //node 관련 코드는 폴더 구조 가져오기
        node = CustomLevelFileSystem.ScanTree();
        nodeList = new Stack<CustomLevelFileSystem.LevelFolderNode>();
        nodeList.Push(node);
        
        initialized = true;
        
        RefreshView();
    }

    public void RefreshView()
    {
        contentRoot.ClearChildren();

        if (nodeList.Count > 1) CreateNavigateBackButton();
        
        //폴더 리스트 보여주기
        foreach (var folders in node.SubFolders)
        {
            Instantiate(navFolderBtnPrefab, contentRoot.transform);
        }
        //레벨 리스트 보여주기
        foreach (var levels in node.LevelFiles)
        {
            Instantiate(navLevelBtnPrefab, contentRoot.transform);
        }
    }

    public string GetCurrentFolderPath()
    {
        return nodeList.Peek().FullPath;
    }

    private void CreateNavigateBackButton()
    {
        Instantiate(navBackBtnPrefab, contentRoot.transform);
    }
    
    #region 버튼함수들

    public void OnClickCreateFolderBtn()
    {
        popupRayLockPanel.SetActive(true);
        createFolderPopup.SetActive(true);
    }

    public void OnClickCreateLevelBtn()
    {
        popupRayLockPanel.SetActive(true);
    }

    public void OnClickExplorerBtn()
    {
        string currentFolderPath = GetCurrentFolderPath();
        System.Diagnostics.Process.Start(currentFolderPath);
    }
    
    #endregion

    public void DisablePopupRayBlocker()
    {
        popupRayLockPanel.SetActive(false);
    }
}
