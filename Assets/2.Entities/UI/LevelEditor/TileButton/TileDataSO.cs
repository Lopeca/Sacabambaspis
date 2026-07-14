using UnityEngine;

[CreateAssetMenu(fileName = "TileDataSO", menuName = "Scriptable Objects/TileDataSO")]
public class TileDataSO : ScriptableObject
{
    public string tileKey;
    //public int tileIndex;
    public GameObject prefab;
    public Sprite editorIcon;
}
