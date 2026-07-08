using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateFolderPopup : MonoBehaviour
{
    public TMP_InputField folderNameInput;

    public void OnClickCreateBtn()
    {
        string path = LevelDesignHubManager.Instance.GetCurrentFolderPath();
        string folderName = folderNameInput.text;
        CustomLevelFileSystem.CreateFolder(path, folderName);
        
        LevelDesignHubManager.Instance.RefreshView();
        LevelDesignHubManager.Instance.DisablePopupRayBlocker();
        gameObject.SetActive(false);
    }

    public void OnClickCancelBtn()
    {
        LevelDesignHubManager.Instance.DisablePopupRayBlocker();
        gameObject.SetActive(false);
    }
}
