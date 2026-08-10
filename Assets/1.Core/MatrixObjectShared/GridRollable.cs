using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(GridMovement))]
public class GridRollable : MonoBehaviour
{
    private GridMovement gridMovement;
    private MatrixObject mo;
    private Vector2Int rollDirection;

    [SerializeField] private MatrixObject aboveObject;  // 디버그 추적용

    private void Awake()
    {
        gridMovement = GetComponent<GridMovement>();
        mo = GetComponent<MatrixObject>();
    }

    public bool CanRoll()
    {
        Vector2Int currentPos = mo.GetPos();

        // 1. 바로 밑 바닥 검사 (바닥 셀이 Filled 상태이고, 둥근 오브젝트여야 함)
        MatrixCell downCell = GetCell(currentPos + Vector2Int.down);
        if (downCell == null || downCell.state != MatrixCell.CellState.Filled)
            return false;
        
        if (downCell.matrixObject == null || !downCell.matrixObject.isRounded)
            return false;

        // 2. 왼쪽 구르기 조건 검사
        if (CheckDirectionCanRoll(currentPos, Vector2Int.left))
        {
            rollDirection = Vector2Int.left;
            return true;
        }

        // 3. 오른쪽 구르기 조건 검사
        if (CheckDirectionCanRoll(currentPos, Vector2Int.right))
        {
            rollDirection = Vector2Int.right;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 지정한 방향(Left/Right)으로 구를 수 있는지 검사 (GC Alloc 0 Bytes)
    /// </summary>
    private bool CheckDirectionCanRoll(Vector2Int currentPos, Vector2Int dir)
    {
        // 옆 칸 검사: 완전히 비어 있어야 함 (Moving 중인 플레이어 차단)
        MatrixCell sideCell = GetCell(currentPos + dir);
        if (sideCell == null || sideCell.state != MatrixCell.CellState.Empty)
            return false;

        // 사선 밑 칸 검사: matrixObject가 null일 뿐만 아니라 state도 완전히 Empty여야 함
        MatrixCell sideDownCell = GetCell(currentPos + dir + Vector2Int.down);
        if (sideDownCell == null || sideDownCell.state != MatrixCell.CellState.Empty || sideDownCell.matrixObject != null)
            return false;

        // 머리 위 사선 오브젝트(낙하 대기 중인 Zonk 등) 검사
        MatrixCell aboveSideCell = GetCell(currentPos + dir + Vector2Int.up);
        if (aboveSideCell != null)
        {
            aboveObject = aboveSideCell.matrixObject;
            // 위 사선 칸에 Filled 상태로 중력 영향을 받는 오브젝트가 있다면 구르기 억제
            if (aboveObject != null && aboveObject.GridGravity != null && aboveSideCell.state == MatrixCell.CellState.Filled)
            {
                return false;
            }
        }

        return true;
    }

    public void ExecuteRoll()
    {
        // speedMultiplier: 0.6~0.7 정도로 설정하면 플레이어 이동보다 빠르게 구르기를 마칩니다.
        gridMovement.ExecuteRollMove(rollDirection, 0.65f);
    }

    private MatrixCell GetCell(Vector2Int pos)
    {
        return GamePlayGridManager.Instance.GetCell(pos);
    }
}