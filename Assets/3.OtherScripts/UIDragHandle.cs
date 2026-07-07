using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [Header("움직일 최상위 팝업 UI창")] [SerializeField]
    private RectTransform targetPopupRect;

    private Vector2 dragOffset;

    void Start()
    {
        // 지정 안 했으면 자동으로 부모의 RectTransform을 타겟으로 잡는 안전장치
        if (targetPopupRect == null) targetPopupRect = transform.parent as RectTransform;
    }

    // 1. 클릭을 시작한 순간: 팝업 중심점과 마우스 커서 사이의 거리(오프셋)를 박제
    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetPopupRect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos
        );

        dragOffset = targetPopupRect.anchoredPosition - localMousePos;
    }

    // 2. 드래그 중인 내내: 마우스 위치에 오프셋을 더해 팝업 좌표를 1:1로 밀어줌
    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetPopupRect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos
        );

        targetPopupRect.anchoredPosition = localMousePos + dragOffset;
    }
}
