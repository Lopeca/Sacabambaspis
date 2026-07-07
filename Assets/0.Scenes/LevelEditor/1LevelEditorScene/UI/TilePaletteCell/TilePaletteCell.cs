using UnityEngine;

public class TilePaletteCell : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    //public GameObject TilePrefab { get; }

    public void OnClick()
    {
        LevelEditorManager.Instance.selectedTile = tilePrefab;
        if(LevelEditorManager.Instance == null) Debug.Log("싱글톤 확인 필요");
        if(LevelEditorManager.Instance.TilePaletteWindow == null) Debug.Log("타일팔레트 윈도우 인지 못함");
        LevelEditorManager.Instance.TilePaletteWindow.SetPointerPosition(this.transform);
    }
}
