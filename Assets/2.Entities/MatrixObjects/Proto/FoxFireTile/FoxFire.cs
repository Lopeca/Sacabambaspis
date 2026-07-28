using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoxFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    [SerializeField] private CollectibleObject collectible;

    [Header("타이밍 설정")]
    [SerializeField] private float activeDuration = 1.0f; // 파티클 유지 및 공격 시간 (1초)
    [SerializeField] private float minWaitTime = 4.0f;     // 최소 대기 시간 (4초)
    [SerializeField] private float maxWaitTime = 5.0f;     // 최대 대기 시간 (5초)
    
    private Coroutine fireLoopCoroutine;

    private void OnEnable()
    {
        // 오브젝트가 활성화되면 루프 시작
        fireLoopCoroutine = StartCoroutine(FoxFireRoutine());
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화되면 코루틴 중단 및 상태 초기화
        if (fireLoopCoroutine != null)
        {
            StopCoroutine(fireLoopCoroutine);
        }

        collectible.isTrap = false;
    }

    private IEnumerator FoxFireRoutine()
    {
        while (true)
        {
            // 1. 4~5초 사이의 랜덤한 대기 시간 계산 후 대기
            float randomCooldown = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(randomCooldown);

            // 2. 파티클 재생 및 피격 가능 상태(ON) 전환
            ps.Play();
            collectible.isTrap = true;


            // 3. 1초 동안 유지
            yield return new WaitForSeconds(activeDuration);

            // 4. 피격 가능 상태(OFF) 전환
            collectible.isTrap = false;
        }
    }
}