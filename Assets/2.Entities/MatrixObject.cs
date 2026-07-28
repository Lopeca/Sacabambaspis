using System;
using System.Collections.Generic;
using UnityEngine;

public class MatrixObject : MonoBehaviour
{
    public enum ExplosionResponse
    {
        Destructible,
        Indestructible,
        Chain
    }

    public int id;  // 디버그용
    [SerializeField] private TileDataSO tileDataSO;
    public TileDataSO TileDataSO => tileDataSO;

    Animator animator;
    public Animator Animator => animator;
    
    private GridMovement gridMovement;
    GridGravity gridGravity;
    public GridGravity GridGravity => gridGravity;
    public CollectibleObject CollectibleObject { get; private set; }
    public IGridInteractable GridInteractable { get; private set; }
    public ExplodeOnDeath ExplodeOnDeath { get; private set; }
    
    public SpriteRenderer SpriteRenderer { get; private set; }

    public ExplosionResponse explosionResponse;
    public bool isRounded;
    public bool isVulnerableToFalling; // 떨어지는 물체에 당하는가

    public int posX;
    public int posY;
    
    List<IGridComponent> gridComponents;

    public Action OnEliminated;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        gridMovement = GetComponent<GridMovement>();
        gridGravity = GetComponent<GridGravity>();
        
        gridComponents = new List<IGridComponent>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        
        CollectibleObject = GetComponent<CollectibleObject>();
        GridInteractable = GetComponent<IGridInteractable>();
        ExplodeOnDeath = GetComponent<ExplodeOnDeath>();
    }

    public Vector2Int GetPos()
    {
        return new Vector2Int(posX, posY);
    }
    public void AppendGridComponent(IGridComponent gridComponent)
    {
        gridComponents.Add(gridComponent);
    }
    public void GridUpdate()
    {
        foreach (var gridComponent in gridComponents)
        {
            gridComponent.GridUpdate();
        }
    }

    public void ForceCompleteTween()
    {
        // 제일 큰 목적은 이동 중인 오브젝트가 폭발에 휩쓸릴 때 폭발범위 바깥의 셀이 정상화되는 것
        // 로직상 이동 완료가 먼저 이루어지고 트윈이 되기 때문에 이동중 폭발에 휩쓸렸다면 출발지는 폭발영역 밖에 있어서 Empty가 보장됨
        if (gridMovement == null) return;
        gridMovement.ForceCompleteMove();
    }
    
    public void ForceCancelTween()
    {
        // 제일 큰 목적은 이동 중인 오브젝트가 폭발에 휩쓸릴 때 폭발범위 바깥의 셀이 정상화되는 것
        // 로직상 이동 완료가 먼저 이루어지고 트윈이 되기 때문에 이동중 폭발에 휩쓸렸다면 출발지는 폭발영역 밖에 있어서 Empty가 보장됨
        if (gridMovement == null)
        {
            Debug.Log("????");
            return;
        }
        gridMovement.ForceCancelMove();
    }

    public void EliminateMatrixObject()
    {
        OnEliminated?.Invoke();
        if(gridMovement != null) gridMovement.ForceCompleteMove();
        MatrixCell currentCell = GamePlayGridManager.Instance.GetCell(posX, posY);
        
        currentCell.Clear();
    }

    public MatrixCell GetCurrentCell()
    {
        return GamePlayGridManager.Instance.GetCell(posX, posY);
    }

    public void MoveToTargetCell(MatrixCell targetCell)
    {
        MatrixCell currentCell = GamePlayGridManager.Instance.GetCell(posX, posY);
        
        // 1. 어떤 오브젝트(this)가 문제인지 gameObject.name을 같이 찍어주면 디버깅이 훨씬 편합니다. 
        Debug.Assert(currentCell.state == MatrixCell.CellState.Filled, 
            $"[{gameObject.name}] 현 오브젝트 위치 ({posX}, {posY}) : 현재 셀 상태가 Filled가 아닙니다. (현재: {currentCell.state})");
        
        Debug.Assert(targetCell.state == MatrixCell.CellState.Empty, 
            $"[{gameObject.name}] 타겟 셀 위치 ({targetCell.GetPosition()}) : 이동하려는 셀이 Empty가 아닙니다. (현재: {targetCell.state})");  
        
        currentCell.Clear();
        targetCell.PutMatrixObject(this);
    }
}
