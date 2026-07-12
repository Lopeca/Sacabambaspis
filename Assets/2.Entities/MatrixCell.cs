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
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private MatrixObject go; // 나중에 MatrixObject로 변경할 수도 있음
    

    public void SetMatrixObject(MatrixObject go)
    {
        this.go = go;
    }

    public MatrixObject GetObject()
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
