using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LevelEditorManager : MonoBehaviour
{
    public static LevelEditorManager instance;
    public static LevelEditorManager Instance
    {
        get
        {
            if (!instance)
            {
                Debug.Log("레벨 에디터 매니저 없는데 호출 중");
                return null;
            }
            return instance;
        }
    }
    private Camera mainCam;

    public Transform matrixRoot;
    
    private const int MAX_WIDTH = 128;
    private const int MAX_HEIGHT = 128;
    
    private MatrixCell[,] mapGrid;
    public MatrixCell[,] MapGrid { get => mapGrid; }

    [Header("플래그 정리")] 
    private bool isSpacePressed;
    private bool isDrawing;
    private bool isErasing;

    [Header("선택된 타일 프리팹")] 
    public GameObject selectedTile;
    
    [Header("UI")]
    [SerializeField] private TilePaletteWindow tilePaletteWindow;
    public TilePaletteWindow TilePaletteWindow => tilePaletteWindow;
    public bool IsInteractingWithUI { get; set; } = false;
    
    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Debug.Log("흐름이 잘못됨 : 레벨에디터 매니저가 사전 생성되어있음");
        }

        mainCam = Camera.main;
        mapGrid = new MatrixCell[MAX_WIDTH, MAX_HEIGHT];
        
        // ★ [추가] 게임 시작 시 격자 무대 미리 생성 및 하이어라키 정리
        GenerateInitialGrid();
    }
    private void Start()
    {
        if (CustomLevelExplorer.Instance.LoadedLevel == null)
        {
            tilePaletteWindow.SaveBtn.gameObject.SetActive(false);
        }
        else
        {
            ConvertLevelDataToMapGrid();
        }
    }

    private void ConvertLevelDataToMapGrid()
    {
        List<TileSaveData> mapData = CustomLevelExplorer.Instance.LoadedLevel.tiles;

        foreach (TileSaveData tileData in mapData)
        {
            selectedTile = TilePrefabPair.Instance.GetPrefab(tileData.tileKey);
            PutTile(tileData.posX, tileData.posY);
        }

        TilePaletteWindow.Init();
    }

  


    void Update()
    {
        if (isSpacePressed) return;
        // [방어벽 2] ★ 마우스가 UI 레이어 위에 있다면 맵 그리기를 완전히 스킵!
        if (IsInteractingWithUI) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        
        if (isDrawing)
        {
            PutTile();
        }
        else if (isErasing)
        {
            EraseTile();
        }
    }
    
    private void PutTile()
    {
        if (!selectedTile) return;
        
        Vector3 mouseWorldPos = MouseWorldPos();

        // 4. 월드 좌표를 격자 인덱스(정수)로 변환 (그리드 스냅)
        // Mathf.FloorToInt를 쓰면 커서가 셀 영역 안(-0.5 ~ +0.5 등) 어디에 있든 정확히 하나의 정수로 묶입니다.
        int posXInt = Mathf.FloorToInt(mouseWorldPos.x);
        int posYInt = Mathf.FloorToInt(mouseWorldPos.y);
        
        int gridX = posXInt + MAX_WIDTH / 2;
        int gridY = posYInt + MAX_HEIGHT / 2;
        
        if (gridX < 0 || gridX >= MAX_WIDTH || gridY < 0 || gridY >= MAX_HEIGHT) return;
        
        if (mapGrid[gridX, gridY].GetObject())
        {
            //Debug.Log($"이미 [{gridX}, {gridY}] 위치에 타일이 존재합니다.");
            return;
        }
        
        // 7. 타일 생성 및 정중앙 배치
        // 중심점이 한가운데인 프리팹이므로, 변환된 정수 좌표(gridX, gridY)에 그대로 배치하면 정확히 격자에 딱 들어맞습니다.
        Vector3 spawnPosition = new Vector3(posXInt + 0.5f, posYInt + 0.5f, 0f);
        MatrixCell cellComponent = mapGrid[gridX, gridY];
        
        MatrixObject spawnedObject = Instantiate(selectedTile, spawnPosition, Quaternion.identity, cellComponent.transform).GetComponent<MatrixObject>();
        
        cellComponent.SetObject(spawnedObject);
        cellComponent.SetPosition(gridX, gridY);
    }

    public void PutTile(int x, int y)
    {
        Vector3 spawnPosition = new Vector3(x + 0.5f - MAX_WIDTH/2, y + 0.5f- MAX_HEIGHT/2);
        MatrixCell cellComponent = mapGrid[x, y];
        
        MatrixObject spawnedObject = Instantiate(selectedTile, spawnPosition, Quaternion.identity, cellComponent.transform).GetComponent<MatrixObject>();
        
        cellComponent.SetObject(spawnedObject);
        cellComponent.SetPosition(x, y);
    }

    private Vector3 MouseWorldPos()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCam.nearClipPlane));
        return mouseWorldPos;
    }

    private void EraseTile()
    {
        Vector3 mouseWorldPos = MouseWorldPos();
        
        int posXInt = Mathf.FloorToInt(mouseWorldPos.x);
        int posYInt = Mathf.FloorToInt(mouseWorldPos.y);
        
        int gridX = posXInt + MAX_WIDTH / 2;
        int gridY = posYInt + MAX_HEIGHT / 2;
        
        Debug.Log("Erase : " + gridX + ", " + gridY);
        if (gridX < 0 || gridX >= MAX_WIDTH || gridY < 0 || gridY >= MAX_HEIGHT) return;
        
        MatrixObject target = mapGrid[gridX, gridY].GetObject();
        if (target)
        {
            Destroy(target.GameObject());
            mapGrid[gridX, gridY].SetObject(null);
        }
        else
        {
            Debug.Log("?");
        }
    }

    private void GenerateInitialGrid()
    {
        // 최상위 루트가 없다면 스크립트가 붙은 오브젝트를 루트로 삼아 방어
        if (!matrixRoot) matrixRoot = this.transform;

        for (int y = 0; y < MAX_HEIGHT; y++)
        {
            // 1. 행(Row) 단위로 묶어줄 부모 오브젝트 생성 (예: "Row 000", "Row 001")
            // D3 포맷은 "000" 형태로 자릿수를 맞춰주어 하이어라키 정렬을 이쁘게 만듭니다.
            GameObject rowObject = new GameObject($"Row(y) {y:D3}");
            rowObject.transform.SetParent(matrixRoot);
            rowObject.transform.localPosition = Vector3.zero;

            for (int x = 0; x < MAX_WIDTH; x++)
            {
                // 2. 정중앙 스냅 위치 계산
                Vector3 spawnPosition = new Vector3(x, y, 0f);

                // 3. 셀 생성 (tilePrefab에 MatrixCell 컴포넌트가 붙어있어야 합니다)
                GameObject cellObj = new GameObject();
                cellObj.AddComponent<MatrixCell>();
                cellObj.transform.SetParent(rowObject.transform);
                cellObj.transform.localPosition = spawnPosition;
                
                float cellPosX = x - (MAX_WIDTH / 2) + 0.5f;
                float cellPosY = y - (MAX_HEIGHT / 2) + 0.5f;
                cellObj.transform.position = new Vector3(cellPosX, cellPosY, 0f); 

                // 4. 이름 지정 (예: "Cell 000_000", "Cell 001_005")
                cellObj.name = $"Cell {x:D3}_{y:D3}";

                // 5. 방금 만든 행(Row) 오브젝트의 자식으로 등록
                cellObj.transform.SetParent(rowObject.transform);

                // 6. 데이터 주입 및 배열 등록
                MatrixCell cellComponent = cellObj.GetComponent<MatrixCell>();
                if (cellComponent != null)
                {
                    // MatrixCell 내부에 좌표를 세팅하는 초기화 함수가 있다면 여기서 호출
                    // cellComponent.SetCoordinate(x, y);

                    mapGrid[x, y] = cellComponent;
                }
            }
        }
    }

    // 이 함수는 직접 바인딩이 아니라 카메라 팬 기능이 없을 때를 전제로 __CameraController.cs 로부터 호출되고 있습니다.
    public void OnGridClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isDrawing = true;
        }
        else if (context.canceled)
            isDrawing = false;
    }

    public void OnGridRightClick(InputAction.CallbackContext context)
    {
        if (context.started)
            isErasing = true;
        else if (context.canceled)
            isErasing = false;
    }

    public void OnSpacePressed(InputAction.CallbackContext context)
    {
        if(context.started)
            isSpacePressed = true;
        else if (context.canceled)
            isSpacePressed = false;
    }

    public void ConvertDataAndSave()
    {
        LevelSaveData loadedData = CustomLevelExplorer.Instance.LoadedLevel;
        loadedData.tiles.Clear();
        
        for (int y = 0; y < mapGrid.GetLength(1); y++){
            for (int x = 0; x < mapGrid.GetLength(0); x++)
            {
                TileSaveData tileSaveData = mapGrid[x, y].ToSaveData();
                if (tileSaveData != null)
                {
                    loadedData.tiles.Add(tileSaveData);
                }
            }
        }
        
        CustomLevelManager.SaveLevel();
    }
}
