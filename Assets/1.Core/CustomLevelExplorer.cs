using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


// 커스텀 레벨 탐색기. 일단은 폴더추가/삭제가 든 에디터용과 단순 플레이용의 기능을 따로 분리하지 않고 만드는 중
public class CustomLevelExplorer : MonoBehaviour
{
    public static CustomLevelExplorer Instance;
    
    private CustomLevelFileSystem.LevelFolderNode node;

    private Stack<CustomLevelFileSystem.LevelFolderNode> nodeList;

    private LevelSaveData loadedLevel;
    public LevelSaveData LoadedLevel => loadedLevel;

    [SerializeField] GameObject contentRoot;
    [SerializeField] private GameObject navBackBtnPrefab;
    [SerializeField] private GameObject navFolderBtnPrefab;
    [SerializeField] private GameObject navLevelBtnPrefab;
    
    [SerializeField] private GameObject popupRayLockPanel;

    [Header("UISets")]
    public GameObject HubUI;
    
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

    // 스캔 함수를 재실행해서 "실제 탐색기에서의 변경을 포착" 할 수 있게 해주는 갱신 함수
    private void RefreshNode()
    {
        string currentPath = GetCurrentNode().FullPath;
        nodeList.Pop();
        CustomLevelFileSystem.LevelFolderNode freshNode = CustomLevelFileSystem.ScanTree(currentPath);
        nodeList.Push(freshNode);

    }
    public void RefreshView()
    {
        contentRoot.ClearChildren();
        
        RefreshNode();
        if (nodeList.Count > 1) CreateNavigateBackButton();
        
        //폴더 리스트 보여주기
        foreach (var folder in nodeList.Peek().SubFolders)
        {
            LevelExplorerButton btn = Instantiate(navFolderBtnPrefab, contentRoot.transform).GetComponent<LevelExplorerButton>();
            btn.SetName(folder.FolderName);
        }
        //레벨 리스트 보여주기
        foreach (var level in nodeList.Peek().LevelFiles)
        {
            LevelExplorerButton btn = Instantiate(navLevelBtnPrefab, contentRoot.transform).GetComponent<LevelExplorerButton>();
            btn.SetName(level);
        }
    }

    public CustomLevelFileSystem.LevelFolderNode GetCurrentNode()
    {
        return nodeList.Peek();
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
        LevelSaveData data = new LevelSaveData();

        string nameBase = "New Level";
        string newLevelName = nameBase;
        
        int i = 0;
        while (true)
        {
            if (i > 0)
            {
                newLevelName = $"{nameBase} ({i})"; 
                // 또는 구버전 스타일: string.Format("{0} ({1})", nameBase, i);
            }
            if (LevelFileNameExist(newLevelName))
            {
                i++;
                continue;
            }

            break;
        }

        data.levelName = newLevelName;
        Debug.Log(data.levelName);
        CustomLevelFileSystem.SaveLevel(data, GetCurrentFolderPath());
        RefreshNode();
        RefreshView();
    }

    public void OnClickExplorerBtn()
    {
        string currentFolderPath = GetCurrentFolderPath();
        System.Diagnostics.Process.Start(currentFolderPath);
    }

    public void OnClickRefreshBtn()
    {
        // 현재 깊은 폴더 유지하려다 터지는 것보다, 깔끔하게 루트부터 다시 스캔하는 게 안전
        nodeList.Clear();
        node = CustomLevelFileSystem.ScanTree(); // 루트부터 완전 재스캔
        nodeList.Push(node);
    
        RefreshView();
    }
    
    #endregion

    private bool LevelFileNameExist(string newLevelName)
    {
        bool isExist = nodeList.Peek().LevelFiles.Exists(e => e == newLevelName);
        Debug.Log(isExist);
        return isExist;
    }
    public void DisablePopupRayBlocker()
    {
        popupRayLockPanel.SetActive(false);
    }

    // 탐색기에서 상위폴더 버튼 클릭 시
    public void NavBack()
    {
        nodeList.Pop();
        RefreshView();
    }

    // 탐색기에서 폴더 버튼 클릭 시
    public void NavFolder(string buttonNameText)
    {
        CustomLevelFileSystem.LevelFolderNode targetNode = nodeList.Peek().SubFolders.Find(e=>e.FolderName==buttonNameText);
        nodeList.Push(targetNode);
        RefreshView();
    }

    #region 씬 여는 함수들
    public void LoadLevelToEditor(string buttonNameText)
    {
        loadedLevel = CustomLevelFileSystem.LoadLevel(Path.Combine(GetCurrentFolderPath(),buttonNameText));
        
        Debug.Log(loadedLevel);
        
        SceneManager.LoadScene("Edit_1LevelEditorScene");
        HubUI.SetActive(false);
    }

    public void LoadLevelEditorHub()
    {
        SceneManager.LoadScene("Edit_0LevelEditorHubScene");
        HubUI.SetActive(true);
    }
    #endregion
}
