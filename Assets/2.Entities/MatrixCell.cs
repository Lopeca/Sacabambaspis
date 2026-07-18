using System;
using UnityEngine;

public enum LookDirection
{
    None = 0, // 방향이 없는 오브젝트 (벽, 빈 공간, 기본 아이템 등)
    Up,
    Down,
    Left,
    Right
}
public class MatrixCell : MonoBehaviour
{
    public enum CellState
    {
        Empty,
        Filled,
        Using,
        Danger
    }
    
    [SerializeField] private int x;
    [SerializeField] private int y;
    public MatrixObject matrixObject; 

    // 그리드 매니저가 핸들링하기 쉽게 그냥 public 설정함
    public CellState state;

    private void Awake()
    {
        state = CellState.Empty;
    }

    
    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(this.x, this.y);
    }

    public TileSaveData ToSaveData()
    {
        if (matrixObject == null) return null;
        
        TileSaveData tileSaveData = new TileSaveData
        {
            tileKey = matrixObject.TileKey,
            posX = x,
            posY = y
        };

        return tileSaveData;
    }

    private void OnDestroy()
    {
        if(matrixObject!=null) 
            Destroy(matrixObject);
    }
    
}
