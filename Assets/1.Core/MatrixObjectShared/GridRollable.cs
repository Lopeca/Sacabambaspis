using System;
using DG.Tweening;
using UnityEngine;

public class GridRollable : MonoBehaviour
{
    private GridMovement gridMovement;
    private MatrixObject mo;
    private Vector2Int rollDirection;

    Tween rollTween;

    private void Awake()
    {
        gridMovement = GetComponent<GridMovement>();
        mo = GetComponent<MatrixObject>();
    }

    public bool CanRoll()
    {
        if (GetCell(mo.GetPos() + Vector2Int.down).state == MatrixCell.CellState.Filled
            && GetCell(mo.GetPos() + Vector2Int.down).matrixObject.isRounded == false)
            return false;
        
        if (GetCell(mo.GetPos() + Vector2Int.left).state == MatrixCell.CellState.Empty
            && GetCell(mo.GetPos() + Vector2Int.left + Vector2Int.down).state == MatrixCell.CellState.Empty)
        {
            rollDirection = Vector2Int.left;
            return true;
        }
        else if (GetCell(mo.GetPos() + Vector2Int.right).state == MatrixCell.CellState.Empty
                 && GetCell(mo.GetPos() + Vector2Int.right + Vector2Int.down).state == MatrixCell.CellState.Empty)
        {
            rollDirection = Vector2Int.right;
            return true;
        }

        return false;
    }

    public void ExecuteRoll()
    {
        gridMovement.ExecuteMove(rollDirection, GridMovement.MoveState.Moving, true);
        ExecuteRollOnly(rollDirection);
    }
    
    /// <summary>
    /// 이동에 관여하지 않고 회전트윈만 담당하겠다는 뜻
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="isPlayerSpeed"></param>
    public void ExecuteRollOnly(Vector2Int direction, bool isPlayerSpeed = false)
    {
        rollDirection = direction;
        
        //duration 동안 한바퀴 회전하는 코드. 왼쪽으로 구르면 반시계, 오른쪽은 시계 방향
        float targetAngle = (rollDirection == Vector2Int.left) ? 360f : -360f;
        float duration = isPlayerSpeed ? GamePlayGridManager.Instance.playerConfigSO.moveDuration : gridMovement.physics.moveDuration;
        
        rollTween = transform.DOLocalRotate(new Vector3(0f, 0f, targetAngle), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 4. 회전이 끝나면 다음 구르기를 위해 로컬 회전값을 깔끔하게 0으로 리셋
                transform.localRotation = Quaternion.identity;
                rollTween = null;
            });
    }

    MatrixCell GetCell(Vector2Int pos)
    {
        return GamePlayGridManager.Instance.GetCell(pos);
    }
    
    private void OnDestroy()
    {
        // 오브젝트 파괴 시 메모리 누수 방지
        if (rollTween != null && rollTween.IsActive())
        {
            rollTween.Kill();
        }
    }
}
