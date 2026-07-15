using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

// 에디터 전용 윈도우로써 레벨 에디터 매니저가 상주한다고 가정할 수 있는 클래스
public class TilePaletteWindow : MonoBehaviour
{
    [SerializeField] private RectTransform tilePointer;
    [SerializeField] private RectTransform contentRoot;

    [SerializeField] private Button saveBtn;
    public Button SaveBtn => saveBtn;
    
    public TMP_Text playButtonText;
    public AllTilesSO allTilesSO;
    public GameObject tileCellPrefab;
    
    public void Init()
    {
        GenerateTileButtons();
        // 직접 실행하는 대신 코루틴을 통해 안전하게 한 프레임 대기 후 첫 타일 선택
        StartCoroutine(SelectFirstTileDeferred());
    }
    private IEnumerator SelectFirstTileDeferred()
    {
        // 1. 레이아웃 리빌드 강제 실행
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        
        // 2. 중요! 다음 프레임까지 대기하여 유니티 내부 UI 캔버스가 완벽히 갱신되도록 만듭니다.
        yield return null; 

        // 3. 자식이 정상적으로 존재하는지 확인 후 안전하게 첫 번째 타일 클릭 시뮬레이션
        if (contentRoot.childCount > 0)
        {
            TilePaletteCell child = contentRoot.transform.GetChild(0).GetComponent<TilePaletteCell>();
            if (child != null)
            {
                child.OnClick(); // 이제 정확한 월드 좌표(position)를 잡습니다!
            }
        }
    }

    private void GenerateTileButtons()
    {
        contentRoot.gameObject.ClearChildren();

        foreach (TileDataSO tileDataSO in allTilesSO.tileDataSOs)
        {
            GameObject go = Instantiate(tileCellPrefab, contentRoot.transform);
            TilePaletteCell cell = go.GetComponent<TilePaletteCell>();
            cell.SetDataSO(tileDataSO);
        }
        
        // 만일을 위한 빈타일 코드. 우클릭 지우기를 지원하고 있기에 필요없을 수도 있음
        // Instantiate(tileCellPrefab, contentRoot.transform);
        
    }


    public void SetPointerPosition(Transform transform1)
    {
        tilePointer.position = transform1.position;
    }

    public void OnClickOpenLevelBtn()
    {
        CustomLevelExplorer.Instance.LoadLevelEditorHub();
    }

    public void OnClickSaveBtn()
    {
        LevelEditorManager.Instance.ConvertDataAndSave();
      
    }

    public void OnClickPlayBtn()
    {
        if (LevelEditorManager.Instance.EditorMode == EditorMode.Edit)
        {
            LevelEditorManager.Instance.StartPlaying();
            playButtonText.text = "Stop Playing";
        }
        else
        {
            LevelEditorManager.Instance.StopPlaying();
            playButtonText.text = "Play";
            
        }
    }

    public void SetPlayButtonTextToPlay()
    {
        playButtonText.text = "Play";
    }

  
}
