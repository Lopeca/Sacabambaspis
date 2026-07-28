using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeOnDeath : MonoBehaviour
{
    private MatrixObject mo;
    private ExplodeElement[] explodeElements;
    [SerializeField] private bool isChainingChicken;
    public bool IsChainingChicken => isChainingChicken;

    private Coroutine chainCoroutine;
    
    GridMovement gridMovement;

    private bool isExploding;
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
    }

    void Start()
    {
        if (GamePlayGridManager.Instance == null)
        {
            Debug.LogError("ExplodeOnDeath: GamePlayGridManager.Instance == null");
            return; // 매니저가 없으면 아래 루프에서 에러가 나므로 리턴 처리
        }

        explodeElements = new ExplodeElement[9];

        // 1. 프리팹 원본의 컴포넌트를 루프 외부에서 딱 한 번만 캐싱합니다.
        // ** 컴포넌트에 걸고 인스턴스화 처음 접함 ;; 
        ExplodeElement prefabComponent = GamePlayGridManager.Instance.explodeEffectElementPrefab.GetComponent<ExplodeElement>();

        if (prefabComponent != null)
        {
            for (int i = 0; i < explodeElements.Length; i++)
            {
                // 2. 컴포넌트 원본을 넣었으므로, Instantiate는 자동으로 ExplodeElement 타입을 반환합니다.
                // 루프 내부에서는 오직 생성 및 트랜스폼 정렬(자식 등록) 연산만 일어납니다.
                explodeElements[i] = Instantiate(prefabComponent, transform);
                
                // 필요하다면 초기화 직후 바로 꺼두기
                explodeElements[i].gameObject.SetActive(false); 
            }
        }
        else
        {
            Debug.LogError("explodeEffectElementPrefab에 ExplodeElement 컴포넌트가 없습니다!");
        }
    }
    
    /// <summary>
    /// 지연시간 후 지정구역 폭발을 의도함
    /// </summary>
    /// <param name="isSpreadingChain"></param>
    public void ExplodeByChain(bool isSpreadingChain = false)
    {
        // 셀과 연결을 끊음
        MatrixCell currentCell = GamePlayGridManager.Instance.GetCell(mo.GetPos());
        currentCell.state = MatrixCell.CellState.Empty;
        currentCell.matrixObject.OnEliminated?.Invoke();
        currentCell.matrixObject = null;
        
        // 매니저에 등록 후 격자 뒤에서 폭발 프로세스
        GamePlayGridManager.Instance.RegisterPendingObject(gameObject);
        chainCoroutine = StartCoroutine(ChainExplode(isSpreadingChain));
    }

    IEnumerator ChainExplode(bool isSpreadingChain)
    {
        if (mo == GamePlayGridManager.Instance.player.MO)
        {
            Debug.Log("플레이어 체인");
        }
        mo.SpriteRenderer.enabled = false;
        yield return new WaitForSeconds(0.3f);
        
        if(isExploding) yield break;
        
        if (mo == GamePlayGridManager.Instance.player.MO)
        {
            Debug.Log("플레이어 체인2");
        }
        // 진행중인 트윈 강제 종료
        if (gridMovement != null)
            gridMovement.ForceCompleteMove();
        
        if (isSpreadingChain) isChainingChicken = true;
        SpawnExplodeElements();
        GamePlayGridManager.Instance.UnregisterPendingObject(gameObject);
        mo.OnEliminated?.Invoke();
        Destroy(gameObject);
    }

    public void Explode(bool isSpreadingChain = false)
    {
        // 진행중인 트윈 강제 종료
        if (gridMovement != null)
            gridMovement.ForceCompleteMove();
        
        if (isSpreadingChain) isChainingChicken = true;
        mo.EliminateMatrixObject();
        
        // 3*3 공간에 공격과 동시에 폭발 엘리먼트 생성(엘리먼트에 체인 속성 넘겨줌)
        SpawnExplodeElements();
        
    }

   private void SpawnExplodeElements()
{
    isExploding = true;
    int count = 0;
    // 연쇄 폭발을 일으킬 인접 폭발 컴포넌트들을 모아둘 리스트
    List<ExplodeOnDeath> chainTargets = new List<ExplodeOnDeath>();

    for (int x = mo.posX - 1; x <= mo.posX + 1; x++)
    {
        for (int y = mo.posY - 1; y <= mo.posY + 1; y++)
        {
            ExplodeElement currentExplodeElement = explodeElements[count];
            MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(x, y);
            
            // 유효성 체크 (이미 파괴된 이펙트 원소면 스킵)
            if (currentExplodeElement == null) 
            {
                count++;
                continue;
            }

            MatrixObject targetCellObject = targetCell.matrixObject;

            if (targetCellObject == null)
            {
                SetupExplodeElement(targetCell, currentExplodeElement);
            }
            else if (targetCellObject.explosionResponse == MatrixObject.ExplosionResponse.Indestructible)
            {
                Destroy(currentExplodeElement.gameObject);
            }
            else
            {
                targetCellObject.ForceCompleteTween();
                
                if (targetCell.state == MatrixCell.CellState.Moving)
                {
                    targetCell.GetMovingObject().ForceCompleteTween();
                }
                
                ExplodeOnDeath sweptObjectExplodeComponent = targetCellObject.ExplodeOnDeath;
                if (sweptObjectExplodeComponent != null)
                {
                    if (isChainingChicken) sweptObjectExplodeComponent.isChainingChicken = true;
                    // ★ 바로 폭발시키지 않고 리스트에 담아둡니다!
                    chainTargets.Add(sweptObjectExplodeComponent);
                    GamePlayGridManager.Instance.RegisterPendingObject(sweptObjectExplodeComponent.gameObject);
                    sweptObjectExplodeComponent.mo.GetCurrentCell().matrixObject = null;
                }
                
                // 내 3x3 엘리먼트 배치를 우선 안전하게 완료함
                SetupExplodeElement(targetCell, currentExplodeElement);
            }
            
            count++;
        }
    }

    // ★ 내 3x3 영역의 이펙트 배치가 전부 끝난 "후"에 연쇄 폭발을 순차적으로 호출!
    foreach (var target in chainTargets)
    {
        if (target != null)
        {
            target.ExplodeByChain(isChainingChicken);
        }
    }
}

    private void SetupExplodeElement(MatrixCell targetCell,
        ExplodeElement currentExplodeElement)
    {
        if (targetCell.matrixObject != null
            && targetCell.matrixObject.TryGetComponent<ExplodeElement>(out var e))
        {
            e.CancelChaining();
            // Debug.Log("CancelChain " + targetCell.GetPosition());
        }
        
        //Debug.Log("폭발셀 : " + targetCell.GetPosition());
        targetCell.Clear();
        targetCell.PutMatrixObject(currentExplodeElement.MO);
        currentExplodeElement.gameObject.SetActive(true);
        targetCell.state = MatrixCell.CellState.Attacking;
        currentExplodeElement.ExplodeCell(isChainingChicken);
    }
}

