using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class TileMaskAnimator : MonoBehaviour
{
    // 기본적으로 플레이어 피직스 SO를 참고해서 속도로 사용할 예정
    public ObjectPhysicsConfigSO speedConfigSO;
    public GameObject mask;
    
    private Tween tileTween;
    private Coroutine tileCoroutine;
    
    int targetTicks;
    private void Awake()
    {
        ResetMask();
        targetTicks = Mathf.Max(1, Mathf.RoundToInt(speedConfigSO.moveDuration / Time.fixedDeltaTime));
    }

    /// <summary>
    /// 마스크를 기본 위치(정중앙)로 초기화합니다.
    /// </summary>
    public void ResetMask()
    {
        if (mask != null)
        {
            // 부모 타일의 정중앙 로컬 좌표(0, 0, 0)로 리셋
            mask.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// 플레이어의 진입 방향에 맞춰 마스크를 이동시키는 연출
    /// </summary>
    /// <param name="direction">플레이어의 진입 방향 (예: 우측 이동 시 Vector2Int.right)</param>
   public void PlayMaskAnimation(Vector2Int direction, Action onAnimationComplete = null)
    {
        if (mask == null) return;
        if (speedConfigSO == null)
        {
            Debug.LogWarning($"{gameObject.name}의 speedConfigSO가 지정되지 않았습니다.");
            return;
        }
        Debug.Log("MaskAni");
        // 이전 동작 중인 트윈과 코루틴 안전하게 중단
        StopActiveMaskRoutine();
        int targetTicks = Mathf.Max(1, Mathf.RoundToInt(speedConfigSO.moveDuration / Time.fixedDeltaTime));
        
        if (direction == Vector2Int.zero)
        {
            tileCoroutine = StartCoroutine(ScaleAnimationRoutine(targetTicks, onAnimationComplete));
            return;
        }

        // 1. SO의 시간(초)을 기반으로 정확한 '목표 Fixed 틱 수' 계산
      
        Vector3 targetLocalPos = new Vector3(direction.x, direction.y, 0f);

        // 2. 틱 기반 코루틴으로 실행
        tileCoroutine = StartCoroutine(MaskAnimationRoutine(targetLocalPos, targetTicks, onAnimationComplete));
    }

    /// <summary>
    /// 지정된 FixedUpdate 틱 동안 오브젝트의 스케일을 0으로 줄이는 연출 및 후처리 코루틴
    /// </summary>
    /// <param name="targetTicks">대기할 FixedUpdate 틱 수</param>
    /// <param name="onAnimationComplete">연출 완수 후 실행할 후처리 콜백 (예: 타일 파괴, 아이템 획득 데이터 처리)</param>
    private IEnumerator ScaleAnimationRoutine(int targetTicks, Action onAnimationComplete)
    {
        // 1. 기존 트윈 및 초기 스케일 정돈 (시작 시 Vector3.one 보장)
        KillActiveTween();
        transform.localScale = Vector3.one;

        // 2. 시각 연출: DOTween으로 Vector3.zero까지 스케일 축소 트윈 실행
        float duration = targetTicks * Time.fixedDeltaTime;
        tileTween = transform.DOScale(Vector3.zero, duration)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Fixed);
   

        // 3. 논리 대기: 정확히 계산된 targetTicks 수만큼 FixedUpdate 대기
        for (int i = 0; i < targetTicks; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // 4. 틱 완수 시점: 트윈 정리 및 스케일 0 강제 Snap (프레임 밀림 방지)
        KillActiveTween();
        transform.localScale = Vector3.zero;

        // 5. 로직 후처리 콜백 실행 (데이터 획득, 타일 Clear 처리 등)
        onAnimationComplete?.Invoke();
        tileCoroutine = null;
    }

    private IEnumerator MaskAnimationRoutine(Vector3 targetLocalPos, int targetTicks, Action onAnimationComplete)
    {
        // 마스크 중앙 초기화
        mask.transform.localPosition = Vector3.zero;
        
        float duration = targetTicks * Time.fixedDeltaTime;
        tileTween = mask.transform.DOLocalMove(targetLocalPos, duration)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Fixed);

        // 논리적 대기: 정확히 지정된 Fixed 틱만큼 대기
        for (int i = 0; i < targetTicks; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // --- 정확히 targetTicks가 지난 시점 ---
        // 뷰(View) 뒷정리: 혹시 트윈이 잔여 프레임 때문에 덜 끝났다면 즉시 타겟 위치로 Snap 후 종료
        KillActiveTween();
        if (mask != null)
        {
            mask.transform.localPosition = targetLocalPos;
        }

        // 데이터/상태(Model) 후처리 실행 (OnComplete 대용)
        onAnimationComplete?.Invoke();
        tileCoroutine = null;
    }

    private void StopActiveMaskRoutine()
    {
        if (tileCoroutine != null)
        {
            StopCoroutine(tileCoroutine);
            tileCoroutine = null;
        }
        KillActiveTween();
    }

    private void KillActiveTween()
    {
        if (tileTween != null && tileTween.IsActive())
        {
            tileTween.Kill();
        }
        tileTween = null;
    }

    /// <summary>
    /// 이 오브젝트가 그리드 매니저나 플레이어에 의해 파괴될 때 호출됩니다.
    /// </summary>
    private void OnDestroy()
    {
        // 4. 오브젝트가 파괴되는 시점에 안전하게 트윈을 정리하여 누수를 막습니다.
        KillActiveTween();
    }
}
