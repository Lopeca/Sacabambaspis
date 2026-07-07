using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LevelEditorCameraController : MonoBehaviour
{
    public Camera _cam;

    private const float baseSize = 6;
    private const float maxSize = 4;
    private const float minSize = 8;

    [Header("카메라 이동 제어 변수")]
    private bool isSpacePressed = false;
    private bool isDragging = false;
    private Vector3 dragStartMousePos;
    private Vector3 dragStartCamPos;

    private Vector3 camInitPos;
    private void Awake()
    {
        camInitPos = _cam.transform.position;
    }

    void LateUpdate()
    {
        // 스페이스바와 마우스 좌클릭이 동시에 유지되고 있을 때만 드래그 이동 처리
        if (isSpacePressed && isDragging)
        {
            HandleCameraPan();
        }
    }
    
    // ★ 핵심 논리: 마우스 이동량만큼 카메라를 1:1로 밀어주기
    private void HandleCameraPan()
    {
        if (Mouse.current == null) return;

        // 현재 마우스의 스크린 좌표(픽셀) 가져오기
        Vector2 currentMousePos = Mouse.current.position.ReadValue();

        // [핵심] 줌 배율(orthographicSize)을 고려하기 위해 Screen이 아닌 World 좌표 기준으로 계산합니다.
        // 드래그를 시작했던 순간의 마우스 월드 좌표와 현재 마우스의 월드 좌표 차이(Delta)를 구합니다.
        Vector3 startWorldPos = _cam.ScreenToWorldPoint(new Vector3(dragStartMousePos.x, dragStartMousePos.y, _cam.nearClipPlane));
        Vector3 currentWorldPos = _cam.ScreenToWorldPoint(new Vector3(currentMousePos.x, currentMousePos.y, _cam.nearClipPlane));

        // 마우스가 움직인 방향의 '반대'로 카메라가 가야 화면이 마우스를 따라오는 느낌이 납니다.
        Vector3 direction = startWorldPos - currentWorldPos;

        // 카메라의 Z축(깊이)은 고정해두고 X, Y축만 이동시킵니다.
        Vector3 targetPos = dragStartCamPos + direction;
        targetPos.z =   _cam.transform.position.z;

        _cam.transform.position = targetPos;
    }

    #region 인풋 시스템 이벤트 연결 함수들

    // 스페이스바 입력 처리 (Started = 누름, Canceled = 뗌)
    public void OnTogglePanMode(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isSpacePressed = true;
        }
        else if (context.canceled)
        {
            isSpacePressed = false;
            isDragging = false; // 스페이스바를 떼면 드래그도 강제 종료
        }
    }

    // 마우스 좌클릭 입력 처리
    public void OnClick(InputAction.CallbackContext context)
    {
        if (isSpacePressed) ReadyCameraPan(context);
        else LevelEditorManager.Instance.OnGridClick(context);
    }

    private void ReadyCameraPan(InputAction.CallbackContext context)
    {
        if (context.started && Mouse.current != null)
        {
            isDragging = true;
            // 드래그를 '시작한 순간'의 마우스 위치(픽셀)와 카메라 위치를 박제(기록)합니다.
            dragStartMousePos = Mouse.current.position.ReadValue();
            dragStartCamPos = _cam.transform.position;
        }
        else if (context.canceled)
        {
            isDragging = false;
        }
    }

    public void ZoomIn(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        _cam.orthographicSize -= 0.5f;
        if (_cam.orthographicSize < maxSize)
            _cam.orthographicSize = maxSize;
    }

    public void ZoomOut(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        _cam.orthographicSize += 0.5f;
        if(_cam.orthographicSize > minSize)
            _cam.orthographicSize = minSize;
    }

    public void ZoomReset(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        _cam.orthographicSize = baseSize;
        _cam.transform.position = camInitPos;

    }

    public void ZoomScroll(InputAction.CallbackContext context)
    {
        // 휠을 굴리는 중(performed) 일 때만 작동
        if (!context.performed) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        // 휠의 Vector2 값을 읽어옵니다.
        Vector2 scrollValue = context.ReadValue<Vector2>();

        // scrollValue.y 가 0보다 크면 위로 굴린 것(확대), 0보다 작으면 아래로 굴린 것(축소)
        if (scrollValue.y > 0f)
        {
            _cam.orthographicSize -= 0.5f;
            if (_cam.orthographicSize < maxSize)
                _cam.orthographicSize = maxSize;
            
        }
        else if (scrollValue.y < 0f)
        {
            _cam.orthographicSize += 0.5f;
            if(_cam.orthographicSize > minSize)
                _cam.orthographicSize = minSize;
        }
    }
    
    #endregion
}
