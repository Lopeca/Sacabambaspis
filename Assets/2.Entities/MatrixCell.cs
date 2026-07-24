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
        Moving,
        Attacking,
        Falling
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
            tileKey = matrixObject.TileDataSO.tileKey,
            posX = x,
            posY = y
        };

        return tileSaveData;
    }

    public void Clear()
    {
        state = CellState.Empty;
        if (matrixObject == null) return;
        Destroy(matrixObject.gameObject);
        matrixObject = null;
    }

    public void PutMatrixObject(MatrixObject matrixObject)
    {
        this.matrixObject = matrixObject;
        matrixObject.transform.SetParent(transform);
        matrixObject.transform.localPosition = Vector3.zero;
        matrixObject.posX =x;
        matrixObject.posY =y;
    }

    private void OnDestroy()
    {
        if(matrixObject!=null) 
            Destroy(matrixObject);
    }
}
