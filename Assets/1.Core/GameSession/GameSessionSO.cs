using UnityEngine;

[CreateAssetMenu(fileName = "GameSessionSO", menuName = "Scriptable Objects/GameSessionSO")]
public class GameSessionSO : ScriptableObject
{
    public int selectedOriginalLevel = 0;
    public string selectedCustomLevelPath = CustomLevelFileSystem.RootPath;
}
