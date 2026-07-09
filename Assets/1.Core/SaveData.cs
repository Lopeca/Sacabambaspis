using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSaveData
{
    public int tileID;
    public int posX;
    public int posY;
}

[System.Serializable]
public class LevelSaveData
{
    public string levelName;
 
    public List<TileSaveData> tiles = new List<TileSaveData>();
}