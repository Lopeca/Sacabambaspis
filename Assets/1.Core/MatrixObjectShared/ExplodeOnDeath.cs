using System;
using UnityEngine;

public class ExplodeOnDeath : MonoBehaviour
{
    private MatrixObject mo;
    private ExplodeElement[] explodeElements;
    [SerializeField] private bool isChainingChicken;
    public bool IsChainingChicken => isChainingChicken;
    
    
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

    public void Explode(bool isChainingChicken = false)
    {
        // 진행중인 트윈 강제 종료
        if (gridMovement != null)
            gridMovement.ForceCompleteMove();
        
        if (isChainingChicken) this.isChainingChicken = true;
        
        // 3*3 공간에 공격과 동시에 폭발 엘리먼트 생성(엘리먼트에 체인 속성 넘겨줌)
        SpawnExplodeElements(this.isChainingChicken);
        
        // 오브젝트 삭제
        Destroy(gameObject);
    }

    private void SpawnExplodeElements(bool isChainingChicken)
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
                    SetupExplodeElement(isChainingChicken, targetCell, currentExplodeElement);
                }
                // 폭발에 휩쓸리지 않는 물체의 공간은 폭발 원소가 생기지 않음
                else if (targetCell.matrixObject.explosionResponse == MatrixObject.ExplosionResponse.Indestructible)
                {
                    Destroy(currentExplodeElement.gameObject);
                }
                else
                {
                    targetCellObject.ForceCompleteTween();
                    
                    if (targetCell.state == MatrixCell.CellState.Moving)
                    {
                        Debug.LogError("셀 상태 사용중, 트윈 완료 조치가 안 된 것으로 보임");
                    }
                    
                    // 폭발 원소에 연쇄 폭발 전이, 치킨 생성 속성도 설정
                    ExplodeOnDeath sweptObjectExplodeComponent = targetCellObject.ExplodeOnDeath;
                    if (sweptObjectExplodeComponent != null)
                    {
                        currentExplodeElement.SetExplodeComponent(sweptObjectExplodeComponent);
                        if (isChainingChicken) sweptObjectExplodeComponent.isChainingChicken = true;
                    }
                    
                    SetupExplodeElement(isChainingChicken, targetCell, currentExplodeElement);
                }
                
                count++;
            }
        }
    }

    private void SetupExplodeElement(bool isChainingChicken, MatrixCell targetCell,
        ExplodeElement currentExplodeElement)
    {
        // 폭발 원소 셀에 소속시키는 과정
        currentExplodeElement.SetPrevObject(targetCell.matrixObject);
        targetCell.PutMatrixObject(currentExplodeElement.MO);
        currentExplodeElement.gameObject.SetActive(true);
        targetCell.state = MatrixCell.CellState.Attacking;
        currentExplodeElement.ExplodeCell(isChainingChicken);
    }
}

