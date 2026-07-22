using System;
using DG.Tweening;
using UnityEngine;

public class GridPushable : MonoBehaviour, IGridInteractable, IGridComponent
{
    private MatrixObject mo;
    GridMovement movement;
    
    private float endureDuration = 0.3f;
    [SerializeField] private float endureCumulativeTime;

    public bool Continuous { get; set; }

    private Tween frontObject;
    private Tween backObject;

    private Vector2Int direction;
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        movement = GetComponent<GridMovement>();
    }

    // 플레이어 입력 페이즈에 들어옴. 물체 고유 AI, 즉 GridUpdate가 있는 곳에서 플래그를 보고 초기화를 담당해줌
    public void Interact(PlayerController player, Vector2Int direction)
    {
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
        Debug.Log("밀기 실행?");
        
        this.direction = direction;
        
        // 1. 셀들을 잠근다
        MatrixCell pusherCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() - direction);
        MatrixCell startCell = GamePlayGridManager.Instance.GetCell(mo.GetPos());
        MatrixCell destCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + direction);
        
        pusherCell.state = MatrixCell.CellState.Moving;
        startCell.state = MatrixCell.CellState.Moving;
        destCell.state = MatrixCell.CellState.Moving;
        
        // 2. 데이터상 이동 완료를 선행
        GamePlayGridManager.Instance.MoveMatrixObjectPosition(mo, direction);
        GamePlayGridManager.Instance.MoveMatrixObjectPosition(GamePlayGridManager.Instance.player.MO, direction);

        //3. 미는 물체 이동 트윈
        movement.PerformMove(destCell.transform.position, null, true);
        if(mo.isRounded) movement.ExecuteRoll(direction, true, true);

        //4. 플레이어 이동 트윈
        GamePlayGridManager.Instance.player.Movement.ForceState(GridMovement.MoveState.Moving);
        GamePlayGridManager.Instance.player.Movement.PerformMove(startCell.transform.position, CompletePush);
    }

    private void CompletePush()
    {
        MatrixCell pusherCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() - direction * 2);
        MatrixCell startCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() - direction);
        MatrixCell destCell = GamePlayGridManager.Instance.GetCell(mo.GetPos());
        
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
