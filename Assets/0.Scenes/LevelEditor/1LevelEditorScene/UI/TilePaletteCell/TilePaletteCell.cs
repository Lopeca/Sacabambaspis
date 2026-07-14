using System;
using UnityEngine;

using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 추가

public class TilePaletteCell : MonoBehaviour
{
    public TileDataSO dataSO;
    
    // UI 요소이므로 SpriteRenderer 대신 Image 사용
    private Image uiImage;

    private void Awake()
    {
        uiImage = GetComponent<Image>();
    }

    public void SetDataSO(TileDataSO tileDataSO)
    {
        dataSO = tileDataSO;
        if (uiImage != null && dataSO != null)
        {
            if(dataSO.editorIcon == null) uiImage.color = Color.black;
            else uiImage.sprite = dataSO.editorIcon;
        }
    }

    public void OnClick()
    {
        if (LevelEditorManager.Instance == null)
        {
            Debug.LogWarning("LevelEditorManager 싱글톤 확인 필요");
            return;
        }
        
        if (LevelEditorManager.Instance.TilePaletteWindow == null)
        {
            Debug.LogWarning("타일팔레트 윈도우 인지 못함");
            return;
        }

        // 선택된 타일 정보 전달 및 UI 포인터 위치 갱신
        LevelEditorManager.Instance.selectedTile = dataSO != null ? dataSO.prefab : null;
        
        // UI 요소의 위치(RectTransform)를 다룰 때는 transform 대신 rectTransform을 전달하는 것이 안전합니다.
        LevelEditorManager.Instance.TilePaletteWindow.SetPointerPosition(this.transform);
    }
}
