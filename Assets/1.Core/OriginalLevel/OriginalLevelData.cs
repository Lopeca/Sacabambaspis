using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "OriginalLevelData", menuName = "Scriptable Objects/OriginalLevel/OriginalLevelData")]
public class OriginalLevelData : ScriptableObject
{
    public string levelName;
    public AssetReference levelAddress;
}
