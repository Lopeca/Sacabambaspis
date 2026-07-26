using System;
using DG.Tweening;
using UnityEngine;

public class GridPushable : MonoBehaviour, IGridInteractable, IGridComponent
{
    private MatrixObject mo;
    GridMovement movement;
    GridGravity gravity;
    
    private float endureDuration = 0.25f;
    [SerializeField] private float endureCumulativeTime;

    public bool Continuous { get; set; }

    private Tween frontObject;
    private Tween backObject;

    private Vector2Int direction;
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        movement = GetComponent<GridMovement>();
        gravity = GetComponent<GridGravity>();
    }

    // 플레이어 입력 페이즈에 들어옴. 물체 고유 AI, 즉 GridUpdate가 있는 곳에서 플래그를 보고 초기화를 담당해줌
    public void Interact(PlayerController player, Vector2Int direction)
    {
        // 중력의 적용을 받는 물체는 수평 방향으로만 밀 기회가 있음
        if (gravity != null && !(direction == Vector2Int.left || direction == Vector2Int.right)) return;
        
        Continuous = true;
        
        if (endureDuration < endureCumulativeTime && CanPush(direction))
            ExecutePush(direction);
        
        endureCumulativeTime += Time.deltaTime;
    }


    private bool CanPush(Vector2Int direction)
    {
        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.posX+direction.x, mo.posY+direction.y);
        return targetCell.state == MatrixCell.CellState.Empty;
    }

    private void ExecutePush(Vector2Int direction)
    {
        endureCumulativeTime = 0;
        this.direction = direction;
        
        // 1. 셀들을 잠근다
        MatrixCell pusherCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() - direction);
        MatrixCell startCell = GamePlayGridManager.Instance.GetCell(mo.GetPos());
        MatrixCell destCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + direction);
        
        pusherCell.state = MatrixCell.CellState.Moving;
        pusherCell.moveStateDirection = direction;
        startCell.state = MatrixCell.CellState.Moving;
        startCell.moveStateDirection = direction;
        destCell.state = MatrixCell.CellState.Moving;
        
        // 2. 데이터상 이동 완료를 선행
        GamePlayGridManager.Instance.MoveMatrixObjectPosition(mo, direction);
        GamePlayGridManager.Instance.MoveMatrixObjectPosition(GamePlayGridManager.Instance.player.MO, direction);

        //3. 미는 물체 이동 트윈
        movement.PerformMove_CustomCompleteAction(destCell.transform.position, true, CompletePush);
        if(mo.isRounded) movement.ExecuteRoll(direction, true, true);

        //4. 플레이어 이동 트윈
        GamePlayGridManager.Instance.player.Movement.ForceState(GridMovement.MoveState.Moving);
        GamePlayGridManager.Instance.player.Movement.PerformMove_CustomCompleteAction(startCell.transform.position, true);
    }

    private void CompletePush()
    {
        Debug.Log("CompletePush MO Pos : " + mo.GetPos());
        MatrixCell pusherCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() - direction * 2);
        MatrixCell startCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() - direction);
        MatrixCell destCell = GamePlayGridManager.Instance.GetCell(mo.GetPos());
        
        Debug.Log("PusherCell pos : " + pusherCell.GetPosition());
        pusherCell.state = MatrixCell.CellState.Empty;
        startCell.state = MatrixCell.CellState.Filled;
        destCell.state = MatrixCell.CellState.Filled;
    }

    public void GridUpdate()
    {
        if(!Continuous)
            endureCumulativeTime = 0;
        
        Continuous = false;
    }
}
