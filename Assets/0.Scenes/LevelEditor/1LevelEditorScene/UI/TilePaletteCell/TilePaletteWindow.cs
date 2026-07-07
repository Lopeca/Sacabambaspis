using System;
using UnityEngine;
using UnityEngine.UI;

// 에디터 전용 윈도우로써 레벨 에디터 매니저가 상주한다고 가정할 수 있는 클래스
public class TilePaletteWindow : MonoBehaviour
{
    [SerializeField] private RectTransform tilePointer;
    [SerializeField] private RectTransform contentRoot;
    
    private void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        TilePaletteCell child = contentRoot.transform.GetChild(0).GetComponent<TilePaletteCell>();
        child.OnClick();
    }



    public void SetPointerPosition(Transform transform1)
    {
        tilePointer.position = transform1.position;
    }

    public void OnClickOpenLevelBtn()
    {
        
    }

    public void OnClickSaveBtn()
    {
        CustomLevelManager.Save(LevelEditorManager.Instance.MapGrid);
    }
}
