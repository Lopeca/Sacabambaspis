using Unity.Cinemachine;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private CinemachineCamera virtualCamera;

    public void SetTarget(Transform target)
    {
        if (virtualCamera == null)
        {
            Debug.LogError("[CameraManager] VirtualCamera가 할당되지 않았습니다.");
            return;
        }

        // 카메라가 플레이어를 따라가도록 설정
        virtualCamera.Target.TrackingTarget = target; // Follow 역할
    }
}
