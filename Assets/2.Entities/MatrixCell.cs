using System;
using UnityEngine;

public enum LookDirection
{
    None = 0,
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
        Receiving,
        Attacking,
        Falling
    }
    
    [SerializeField] private int x;
    [SerializeField] private int y;
    public MatrixObject matrixObject; 

    // ★ [1] 추적하고 싶은 특정 셀 좌표를 지정하세요 (예: Vector2Int.zero)
    // -1로 두면 모든 셀의 변경 로그를 출력합니다.
    [Header("Debug Inspector")]
    [SerializeField] private bool enableStateLog = true;
    [SerializeField] private Vector2Int debugTargetPos = new Vector2Int(-2, -2); 

    // ★ [2] private 캡슐화 + 프로퍼티화
    [SerializeField] private CellState _state;
    
    public CellState state
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                // 타겟 좌표 검사 (debugTargetPos가 -1, -1이거나 현재 좌표와 일치할 때)
                
                if (enableStateLog && (debugTargetPos.x == -1 || (debugTargetPos.x == x && debugTargetPos.y == y)))
                {
                    // 프레임수(Time.frameCount), 좌표, 이전상태 -> 변경상태, 그리고 호출한 스크립트 위치 출력
                    Debug.Log($"<color=#FFD700>[Frame {Time.frameCount}] Cell({x},{y}) State Changed:</color> " +
                              $"<b>{_state}</b> ➔ <color=#00FF00><b>{value}</b></color>\n" +
                              $"<color=#888888>CallStack: {UnityEngine.StackTraceUtility.ExtractStackTrace()}</color>");
                }

                _state = value;
            }
        }
    }

    public Vector2Int moveStateDirection;

    private void Awake()
    {
        _state = CellState.Empty;
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
        matrixObject.posX = x;
        matrixObject.posY = y;
    }

    private void OnDestroy()
    {
        if (matrixObject != null) 
            Destroy(matrixObject);
    }

    public bool HasPlayer()
    {
        if (matrixObject == null)
        {
            if (state == CellState.Moving && GetMovingObject() == GamePlayGridManager.Instance.player.MO) return true;
            return false;
        }
        return matrixObject == GamePlayGridManager.Instance.player.MO;
    }

    public MatrixObject GetMovingObject()
    {
        Debug.Assert(state == CellState.Moving, $"[Cell] Moving 상태가 아닌 셀({x}, {y})에서 GetMovingObject를 호출했습니다!");
        if (state != CellState.Moving) return null;
        return GamePlayGridManager.Instance.GetCell(x + moveStateDirection.x, y + moveStateDirection.y).matrixObject;
    }
}