using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExplorerButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonName;
    public void SetName(string name)
    {
        buttonName.text = name;
        
    }

    public void OnClickNavBackButton()
    {
        CustomLevelExplorer.Instance.NavBack();
    }

    public void OnClickFolderButton()
    {
        CustomLevelExplorer.Instance.NavFolder(buttonName.text);
    }

    public void OnClickLevelButton()
    {
        CustomLevelExplorer.Instance.LoadLevelToEditor(buttonName.text);
    }
    
}
