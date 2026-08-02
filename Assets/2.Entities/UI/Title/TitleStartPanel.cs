using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleStartPanel : MonoBehaviour
{
    public void OnClickAdventureBtn()
    {
        
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
