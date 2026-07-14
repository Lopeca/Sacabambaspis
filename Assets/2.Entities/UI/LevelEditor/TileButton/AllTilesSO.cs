using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllTilesSO", menuName = "Scriptable Objects/AllTilesSO")]
public class AllTilesSO : ScriptableObject
{
    public List<TileDataSO> tileDataSOs;

    public GameObject GetPrefab(string tileDataTileKey)
    {
        return tileDataSOs.Find(x => x.tileKey == tileDataTileKey).prefab;
    }
}
