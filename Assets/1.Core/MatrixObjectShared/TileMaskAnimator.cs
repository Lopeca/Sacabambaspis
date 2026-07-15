using System;
using DG.Tweening;
using UnityEngine;

public class TileMaskAnimator : MonoBehaviour
{
    // 기본적으로 플레이어 피직스 SO를 참고해서 속도로 사용할 예정
    public ObjectPhysicsConfigSO speedConfigSO;
    public GameObject mask;

    private void Awake()
    {
        ResetMask();
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

        // 1. 연출 전 마스크 위치를 중앙으로 초기화 (이전 트윈이 꼬이는 것 방지)
        mask.transform.localPosition = Vector3.zero;

        // 2. 플레이어의 진입 방향으로 마스크가 이동해야 할 목표 '로컬' 좌표 계산
        // 격자 한 칸 크기가 1이므로, 방향 벡터를 그대로 목적지 오프셋으로 사용합니다.
        Vector3 targetLocalPos = new Vector3(direction.x, direction.y, 0f);

        // 3. SO에 정의된 moveDuration 동안 마스크를 로컬 좌표계 기준으로 부드럽게 이동시킵니다.
        // Ease.Linear를 쓰면 플레이어의 등속 격자 이동과 완벽하게 싱크가 맞습니다.
        mask.transform.DOLocalMove(targetLocalPos, speedConfigSO.moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 4. 연출이 끝나면 마스크를 다시 원래대로 되돌리거나, 
                // 타일 자체를 파괴(Destroy)하는 등 후처리 로직을 여기에 작성합니다.
                onAnimationComplete?.Invoke();
            });
    }
}
