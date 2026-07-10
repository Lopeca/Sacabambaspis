using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TileSaveData
{
    public string tileKey;
    public LookDirection initDirection = LookDirection.None; //새로 추가
    public int posX;
    public int posY;
}

[System.Serializable]
public class LevelSaveData
{
    public string levelName;
 
    public List<TileSaveData> tiles = new List<TileSaveData>();
}