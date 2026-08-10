using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleStartPanel : MonoBehaviour
{
    [SerializeField] SceneTransitionSO _sceneTransitionSO;
    public void OnClickAdventureBtn()
    {
        _sceneTransitionSO.LoadSceneWithFade("AdventureScene");
    }

    public void OnClickCustomMapBtn()
    {
        
    }

    public void OnClickManualBtn()
    {
        TitleUI.Instance.OnClickManualBtn();
    }

    public void OnClickEditorBtn()
    {
        SceneManager.LoadScene("Edit_0LevelEditorHubScene");
    }
    
    public void OnClickUndoBtn()
    {
        TitleUI.Instance.ShowMainPanel();
    }
}
