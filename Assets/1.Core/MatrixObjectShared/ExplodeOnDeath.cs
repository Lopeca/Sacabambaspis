using UnityEngine;

public class ExplodeOnDeath : MonoBehaviour
{
    private ExplodeElement[] explodeElements;

    
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

    public void Explode()
    {
        
    }
}

