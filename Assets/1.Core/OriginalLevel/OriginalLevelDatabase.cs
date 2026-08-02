using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OriginalLevelDatabase", menuName = "Scriptable Objects/OriginalLevel/OriginalLevelDatabase")]
public class OriginalLevelDatabase : ScriptableObject
{
    public List<OriginalLevelData> originalLevels;
}
