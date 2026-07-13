using System;
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
    
    private void Awake()
    {
        instance = this;
    }

    public void SetGrid(MatrixCell[,] mapGrid)
    {
        this.mapGrid = mapGrid;
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
                }
            }
        }
        
        // 매트릭스에 오브젝트 채워넣기
        foreach (TileSaveData matrixObject in objects)
        {
            int cellPosX = matrixObject.posX - minX;
            int cellPosY = matrixObject.posY - minY;
            MatrixCell targetCell = mapGrid[cellPosX, cellPosY];
            GameObject targetPrefab = TilePrefabPair.Instance.GetPrefab(matrixObject.tileKey);
            MatrixObject clonedMO = Instantiate(targetPrefab, targetCell.transform.position, Quaternion.identity, targetCell.transform).GetComponent<MatrixObject>();
            clonedMO.posX = cellPosX;
            clonedMO.posY = cellPosY;
            
            targetCell.SetMatrixObject(clonedMO);

            if (clonedMO.TryGetComponent<PlayerController>(out var pc))
            {
                player = pc; 
                player.SetReady();
            }
        }
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

    public bool TryReserveMove(MatrixObject mo, Vector2Int startPos, Vector2Int destPos)
    {
        if (mapGrid[destPos.x, destPos.y].state != MatrixCell.CellState.Empty)
        {
            Debug.Log("뭔가 있음");
            return false;
        }
        
        MatrixCell startCell = mapGrid[startPos.x, startPos.y];
        MatrixCell destCell = mapGrid[destPos.x, destPos.y];
        
        startCell.state = MatrixCell.CellState.Using;
        destCell.state = MatrixCell.CellState.Using;
        
        destCell.SetMatrixObject(startCell.GetMatrixObject());
        startCell.SetMatrixObject(null);
        return true;
    }
    public bool TryReserveAttack(MatrixObject mo, Vector2Int startPos, Vector2Int destPos)
    {
        if(mapGrid[destPos.x, destPos.y].state != MatrixCell.CellState.Empty) return false;
        
        MatrixCell startCell = mapGrid[startPos.x, startPos.y];
        MatrixCell destCell = mapGrid[destPos.x, destPos.y];
        
        startCell.state = MatrixCell.CellState.Using;
        destCell.state = MatrixCell.CellState.Danger;
        
        destCell.SetMatrixObject(startCell.GetMatrixObject());
        startCell.SetMatrixObject(null);
        return true;
    }

    public void CompleteMove(Vector2Int startPos, Vector2Int destPos)
    {
        MatrixCell startCell = mapGrid[startPos.x, startPos.y];
        MatrixCell destCell = mapGrid[destPos.x, destPos.y];
        
        startCell.state = MatrixCell.CellState.Empty;
        destCell.state = MatrixCell.CellState.Filled;
    }
}
