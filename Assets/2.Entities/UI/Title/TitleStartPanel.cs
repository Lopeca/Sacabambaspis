using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleStartPanel : MonoBehaviour
{
    [SerializeField] SceneTransitionSO _sceneTransitionSO;
    [SerializeField] SceneAsset adventureScene;
    public void OnClickAdventureBtn()
    {
        _sceneTransitionSO.LoadSceneWithFade(adventureScene.name);
    }

    public void OnClickCustomMapBtn()
    {
        
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
