using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectLevelForEditManager : MonoBehaviour
{
    public static SelectLevelForEditManager Instance;
    
    private CustomLevelFileSystem.LevelFolderNode node;

    private Stack<CustomLevelFileSystem.LevelFolderNode> nodeList;
    
    [SerializeField] GameObject contentRoot;
    [SerializeField] private GameObject navBackBtnPrefab;
    [SerializeField] private GameObject navFolderBtnPrefab;
    [SerializeField] private GameObject navLevelBtnPrefab;
    
    [SerializeField] private GameObject popupRayLockPanel;


    
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
        
        node = CustomLevelFileSystem.ScanTree();
        nodeList = new Stack<CustomLevelFileSystem.LevelFolderNode>();
        nodeList.Push(node);
        
        initialized = true;
        
        RefreshView();
    }

    private void RefreshView()
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

    private void CreateNavigateBackButton()
    {
        Instantiate(navBackBtnPrefab, contentRoot.transform);
    }
    
    #region 버튼함수들

    public void OnClickCreateFolderBtn()
    {
        popupRayLockPanel.SetActive(true);
    }

    public void OnClickCreateLevelBtn()
    {
        popupRayLockPanel.SetActive(true);
    }
    
    #endregion
}
