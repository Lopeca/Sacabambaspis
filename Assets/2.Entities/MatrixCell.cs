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
    [SerializeField] private MatrixObject go; 

    // 그리드 매니저가 핸들링하기 쉽게 그냥 public 설정함
    public CellState state;

    private void Awake()
    {
        state = CellState.Empty;
    }


    public void SetMatrixObject(MatrixObject go)
    {
        this.go = go;
    }



    public MatrixObject GetMatrixObject()
    {
        return go;
    }

    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public TileSaveData ToSaveData()
    {
        if (go == null) return null;
        
        TileSaveData tileSaveData = new TileSaveData
        {
            tileKey = go.TileKey,
            posX = x,
            posY = y
        };

        return tileSaveData;
    }

    private void OnDestroy()
    {
        if(go!=null) 
            Destroy(go);
    }
}
