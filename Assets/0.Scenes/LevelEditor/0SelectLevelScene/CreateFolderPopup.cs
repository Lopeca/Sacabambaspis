using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateFolderPopup : MonoBehaviour
{
    public TMP_InputField folderNameInput;

    public void OnClickCreateBtn()
    {
        string path = CustomLevelExplorer.Instance.GetCurrentFolderPath();
        string folderName = folderNameInput.text;
        CustomLevelFileSystem.CreateFolder(path, folderName);
        
        CustomLevelExplorer.Instance.RefreshView();
        CustomLevelExplorer.Instance.DisablePopupRayBlocker();
        gameObject.SetActive(false);
    }

    public void OnClickCancelBtn()
    {
        CustomLevelExplorer.Instance.DisablePopupRayBlocker();
        gameObject.SetActive(false);
    }
}
