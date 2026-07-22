using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 생성할 일 없는 플레이씬 고유 싱글톤. 방어코드 없이, 오류 발생하면 코드로 해결할 게 아니라 에디터에서 무결하게 맞춰갈 것
public class GamePlayGridManager : MonoBehaviour
{
    static GamePlayGridManager instance;
    public static GamePlayGridManager Instance => instance;

    
    LevelSaveData loadedLevelData;

    // 그리드 셀들을 담는 부모
    public Transform playGridRoot;
    
    private MatrixCell[,] mapGrid;
    
    public PlayerController player;
    public AllTilesSO allTilesSO;
    public ObjectPhysicsConfigSO playerConfigSO;
    
    [Header("범용 프리팹")]
    public GameObject explodeEffectElementPrefab;   // 매니저 본업의 영역은 아니지만 사소해서 매니저한테 맡기는 프리팹 참조용 필드
    public GameObject chickenPrefab;
    
    public bool isPlaying;
    
    [Header("스코어보드")]
    [SerializeField] int requiredChickenCount;
    // 플레이 타임
    // 레드 디스크 수

    public event Action OnGameOver;
    private void Awake()
    {
        instance = this;

        isPlaying = false;
        
        ExitObject.OnTryExit += ExitEventListener;
    }

    private void Update()
    {
        //isPlaying 반복 검사는 중간 페이즈에 게임이 끝날 경우 모든 생명주기를 돌 필요가 없기 때문. 턴제게임이 꼭 내턴이 끝나지 않아도 상대가 무너지면 중간에 끝내는 것처럼, 한 주기 안에서 반복 체크하는 건 자연스러운 일이라고 함
       
        
        if (!isPlaying) return;
        
        // 1. 오브젝트들 자연 업데이트 
        for (int x = 0; x < mapGrid.GetLength(0); x++)
        {
            for (int y = mapGrid.GetLength(1)-1; y >=0; y--)
            {
                var cell = mapGrid[x, y];
                if (cell.state == MatrixCell.CellState.Filled)
                {
                    cell.matrixObject.GridUpdate();
                }
            }
        }
        
        if (!isPlaying) return;
        
        // 2. 플레이어 인풋 처리
        if (player != null) player.PlayerUpdate();
    }
    
    public void SetLevelData(LevelSaveData levelSaveData)
    {
        loadedLevelData = levelSaveData;
    }

    public void LoadCustomLevel()
    {
        loadedLevelData = CustomLevelExplorer.Instance.LoadedLevel;
        ConvertLevelDataToGrid();
    }

    private void ConvertLevelDataToGrid()
    {
        List<TileSaveData> objects = loadedLevelData.tiles;
        if (objects == null || objects.Count == 0) return;

        // LINQ 구문으로 최솟값, 최댓값 찾기
        int minX = objects.Min(t => t.posX);
        int maxX = objects.Max(t => t.posX);
        int minY = objects.Min(t => t.posY);
        int maxY = objects.Max(t => t.posY);
    
        // 이 값들을 활용해 런타임 격자 크기(Width, Height)를 동적으로 계산할 수 있습니다.
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        
        // 사이즈에 맞게 그리드 생성 (레벨에디터매니저로부터 카피)
        mapGrid = new MatrixCell[width, height];
        
        for (int y = 0; y < height; y++)
        {
            // 1. 행(Row) 단위로 묶어줄 부모 오브젝트 생성 (예: "Row 000", "Row 001")
            // D3 포맷은 "000" 형태로 자릿수를 맞춰주어 하이어라키 정렬을 이쁘게 만듭니다.
            GameObject rowObject = new GameObject($"Row(y) {y:D3}");
            rowObject.transform.SetParent(playGridRoot);
            rowObject.transform.localPosition = Vector3.zero;

            for (int x = 0; x < width; x++)
            {
                // 2. 정중앙 스냅 위치 계산
                Vector3 spawnPosition = new Vector3(x, y, 0f);

                // 3. 셀 생성 (tilePrefab에 MatrixCell 컴포넌트가 붙어있어야 합니다)
                GameObject cellObj = new GameObject();
                cellObj.AddComponent<MatrixCell>();
                cellObj.transform.SetParent(rowObject.transform);
                cellObj.transform.localPosition = spawnPosition;
                
                float cellPosX = x + 0.5f;
                float cellPosY = y + 0.5f;
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
                    cellComponent.SetPosition(x,y);
                }
            }
        }

        int id = 0;
        // 매트릭스에 오브젝트 채워넣기
        foreach (TileSaveData matrixObject in objects)
        {
            int cellPosX = matrixObject.posX - minX;
            int cellPosY = matrixObject.posY - minY;
            MatrixCell targetCell = mapGrid[cellPosX, cellPosY];
            GameObject targetPrefab = allTilesSO.GetPrefab(matrixObject.tileKey);
            MatrixObject clonedMO = Instantiate(targetPrefab, targetCell.transform.position, Quaternion.identity, targetCell.transform).GetComponent<MatrixObject>();
            clonedMO.posX = cellPosX;
            clonedMO.posY = cellPosY;
            
            targetCell.matrixObject = clonedMO;
            targetCell.state = MatrixCell.CellState.Filled;
            targetCell.matrixObject.id = id++;

            if (clonedMO.CompareTag("Player"))
            {
                player = clonedMO.GetComponent<PlayerController>(); 
                player.SetReady();
                player.OnDeath += HandlePlayerDeath;
            }
        }
    }

    private void ExitEventListener()
    {
        player = null;
        StartCoroutine(SendGameOverAfterSeconds(1.5f));
    }

    public void ClearGrid()
    {
        playGridRoot.gameObject.ClearChildren();

        // 2. 런타임 데이터 배열 청소
        if (mapGrid != null)
        {
            // 배열 자체를 메모리에서 해제하거나 null 처리하여 가비지 컬렉터(GC)가 수거하도록 유도
            Array.Clear(mapGrid, 0, mapGrid.Length);
            mapGrid = null;
        }
    }

    public MatrixCell GetCell(int x, int y)
    {
        return mapGrid[x, y];
    }
    public MatrixCell GetCell(Vector2Int pos)
    {
        return mapGrid[pos.x, pos.y];
    }

    public void ClearCell(int x, int y)
    {
        mapGrid[x, y].matrixObject = null;
        mapGrid[x, y].state = MatrixCell.CellState.Empty;
    }

    public void ClearCell(Vector2Int pos)
    {
        ClearCell(pos.x, pos.y);
    }

    /// <summary>
    /// 주의 : 셀 상태를 바꾸지 않음. 로직상 필요해서 셀의 MatrixObject만 우선적으로 옮겨줌. 트윈 등 이동절차 완료 후 셀 상태가 별도로 다뤄지기에 기능이 분리됨
    /// 즉 셀 A에서 B로 이동할 때 A가 Empty가 되지 않고 B가 Filled 상태가 되지 않으니 주의해서 다룰 것
    /// </summary>
    /// <param name="matrixObject"></param>
    /// <param name="destX"></param>
    /// <param name="destY"></param>
    public void MoveMatrixObjectPosition(MatrixObject matrixObject, int destX, int destY)
    {
        MatrixCell prevCell = mapGrid[matrixObject.posX,matrixObject.posY];
        MatrixCell destCell = mapGrid[destX,destY];

        if (destCell.matrixObject != null)
        {
            Debug.LogError("로직 오류 : 오브젝트가 이미 있는 칸으로의 이동이 감지됨. 이동 가능 여부 검사 로직 확인 필요함.");
            Debug.Break();
            return;
        }

        destCell.matrixObject = matrixObject;
        prevCell.matrixObject = null;
        
        matrixObject.posX = destX;
        matrixObject.posY = destY;
    }

    public void MoveMatrixObjectPosition(MatrixObject matrixObject, Vector2Int direction)
    {
        int destX = matrixObject.posX + direction.x;
        int destY = matrixObject.posY + direction.y;
        
        MoveMatrixObjectPosition(matrixObject, destX, destY);
    }
    
    public void SetCellState(Vector2Int targetPos, MatrixCell.CellState state)
    {
        MatrixCell cell = mapGrid[targetPos.x, targetPos.y];
        cell.state = state;
    }

    void HandlePlayerDeath()
    {
        Debug.Log("[PlayGridManager] 플레이어 사망으로 인한 게임 종료. 1.5초 후 종료됩니다.");
        player.OnDeath -= HandlePlayerDeath;
        StartCoroutine(SendGameOverAfterSeconds(1.5f));
    }

    IEnumerator SendGameOverAfterSeconds(float f)
    {
        yield return new WaitForSeconds(f);
        OnGameOver?.Invoke();
    }

    private void OnDestroy()
    {
        if(player != null) player.OnDeath -= HandlePlayerDeath;
        ExitObject.OnTryExit -= ExitEventListener;
    }
}
