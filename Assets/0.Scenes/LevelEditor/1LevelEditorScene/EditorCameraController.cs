using UnityEngine;

public class EditorCameraController : MonoBehaviour
{
    [Header("Tracking Settings")]
    [SerializeField] private float smoothSpeed = 0.125f;
    private Transform playerTarget;
    private Vector3 editModePosition;
    private bool isPlayMode = false;

    private void Start()
    {
        Debug.Log($"[Camera] Start 실행됨. Instance ID: {GetHashCode()}", gameObject);
        // 에디터 매니저가 존재할 때만 이벤트를 구독 (느슨한 결합)
        if (LevelEditorManager.Instance != null)
        {
            LevelEditorManager.OnPlayModeStarted += EnablePlayModeCamera;
            LevelEditorManager.OnPlayModeStopped += DisablePlayModeCamera;
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"[Camera] OnDestroy 실행됨. Instance ID: {GetHashCode()}");

        if (LevelEditorManager.Instance != null)
        {
            LevelEditorManager.OnPlayModeStarted -= EnablePlayModeCamera;
            LevelEditorManager.OnPlayModeStopped -= DisablePlayModeCamera;
        }
    }

    private void LateUpdate()
    {
        if (!isPlayMode) return;

        // 플레이 모드일 때 플레이어 타겟 추적
        if (playerTarget == null)
        {
            return;
        }

        Vector3 desiredPosition = new Vector3(playerTarget.position.x, playerTarget.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }

    private void EnablePlayModeCamera()
    {
        // 1. 에디터 상태의 카메라 원래 위치를 기억
        editModePosition = transform.position;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTarget = player.transform;
        isPlayMode = true;
    }

    private void DisablePlayModeCamera()
    {
        isPlayMode = false;
        playerTarget = null;

        // 2. 에디터 원래 위치로 복구
        transform.position = editModePosition;
    }
}
