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

    public bool debugTrace;

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
        ExplodeElement prefabComponent =
            GamePlayGridManager.Instance.explodeEffectElementPrefab.GetComponent<ExplodeElement>();

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
        // 트윈 및 코루틴 즉시 정지 (셀 상태 복구 시도 안 함)
        if (gridMovement != null) 
            gridMovement.ForceCompleteMove();

        mo.OnEliminated?.Invoke();

        if (debugTrace)
        {
            Debug.Log("mo Pos : " + mo.GetPos());
        }
            
        // 셀 연결 해제
        MatrixCell currentCell = GamePlayGridManager.Instance.GetCell(mo.GetPos());
        if (mo.GridCreature != null) mo.GridCreature.IsLive = false;
        GamePlayGridManager.Instance.RegisterPendingObject(gameObject);
    
        currentCell.matrixObject = null; 
        currentCell.state = MatrixCell.CellState.Empty;
    
        chainCoroutine = StartCoroutine(ChainExplode(isSpreadingChain));
    }
    
    IEnumerator ChainExplode(bool isSpreadingChain)
    {
        mo.SpriteRenderer.enabled = false;
        yield return new WaitForSeconds(0.3f);
        
        if (isExploding) yield break;

        if (isSpreadingChain) isChainingChicken = true;
        
        mo.EliminateMatrixObject();
        
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
        SoundManager.Instance.PlayExplodeSFX(transform.position);
        int count = 0;

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

                    // if (targetCell.state == MatrixCell.CellState.Moving)
                    // {
                    //     targetCell.GetMovingObject().ForceCompleteTween();
                    // }

                    ExplodeOnDeath sweptObjectExplodeComponent = targetCellObject.ExplodeOnDeath;
                    if (sweptObjectExplodeComponent != null)
                    {
                        if (isChainingChicken) sweptObjectExplodeComponent.isChainingChicken = true;

                        sweptObjectExplodeComponent.ExplodeByChain(isChainingChicken);
                    }

                    // 내 3x3 엘리먼트 배치를 우선 안전하게 완료함
                    currentExplodeElement.MO.id = count;
                    SetupExplodeElement(targetCell, currentExplodeElement);
                }

                count++;
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
        
        if(targetCell.matrixObject != null) targetCell.matrixObject.EliminateMatrixObject();    // 폭발물은 앞서 pending으로 옮겨서 비어있음 
        targetCell.PutMatrixObject(currentExplodeElement.MO);
        currentExplodeElement.gameObject.SetActive(true);
        targetCell.state = MatrixCell.CellState.Attacking;
        currentExplodeElement.ExplodeCell(isChainingChicken);
    }
}