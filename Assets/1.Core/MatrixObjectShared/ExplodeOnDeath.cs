using System;
using System.Collections;
using UnityEngine;

public class ExplodeOnDeath : MonoBehaviour
{
    private MatrixObject mo;
    private ExplodeElement[] explodeElements;
    [SerializeField] private bool isChainingChicken;
    public bool IsChainingChicken => isChainingChicken;

    private Coroutine chainCoroutine;
    
    GridMovement gridMovement;

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
        currentCell.matrixObject = null;
        
        // 매니저에 등록 후 격자 뒤에서 폭발 프로세스
        GamePlayGridManager.Instance.RegisterPendingObject(gameObject);
        chainCoroutine = StartCoroutine(ChainExplode(isSpreadingChain));
    }

    IEnumerator ChainExplode(bool isSpreadingChain)
    {
        mo.SpriteRenderer.enabled = false;
        yield return new WaitForSeconds(0.3f);
        // 진행중인 트윈 강제 종료
        if (gridMovement != null)
            gridMovement.ForceCompleteMove();
        
        if (isSpreadingChain) isChainingChicken = true;
        SpawnExplodeElements();
        GamePlayGridManager.Instance.UnregisterPendingObject(gameObject);
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
        int count = 0;
        for (int x = mo.posX - 1; x <= mo.posX + 1; x++)
        {
            for (int y = mo.posY - 1; y <= mo.posY + 1; y++)
            {
                ExplodeElement currentExplodeElement = explodeElements[count];
                MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(x, y);
                MatrixObject targetCellObject = targetCell.matrixObject;

                if (targetCell.matrixObject == null)
                {
                    SetupExplodeElement(targetCell, currentExplodeElement);
                }
                // 폭발에 휩쓸리지 않는 물체의 공간은 폭발 원소가 생기지 않음
                else if (targetCellObject.explosionResponse == MatrixObject.ExplosionResponse.Indestructible)
                {
                    Destroy(currentExplodeElement.gameObject);
                }
                else
                {
                    Debug.Log("물체 감지 - ID : " + targetCell.matrixObject.id);
                    targetCellObject.ForceCompleteTween();
                    
                    if (targetCell.state == MatrixCell.CellState.Moving)
                    {
                        Debug.LogError("셀 상태 사용중, 트윈 완료 조치가 안 된 것으로 보임");
                    }
                    
                    ExplodeOnDeath sweptObjectExplodeComponent = targetCellObject.ExplodeOnDeath;
                    if (sweptObjectExplodeComponent != null)
                    {
                        if (isChainingChicken) sweptObjectExplodeComponent.isChainingChicken = true;
                        sweptObjectExplodeComponent.ExplodeByChain(isChainingChicken);

                        // Debug.Log("연쇄폭발설정 ㅣ id : " + sweptObjectExplodeComponent.mo.id + " ");
                    }
                    
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
        }
        targetCell.Clear();
        targetCell.PutMatrixObject(currentExplodeElement.MO);
        currentExplodeElement.gameObject.SetActive(true);
        targetCell.state = MatrixCell.CellState.Attacking;
        currentExplodeElement.ExplodeCell(isChainingChicken);
    }
}

