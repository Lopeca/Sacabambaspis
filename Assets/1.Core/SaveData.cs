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

    public bool autoCountChicken;
    public int requiredChickenCount;
 
    public List<TileSaveData> tiles = new List<TileSaveData>();
}

[System.Serializable]
public class UserSaveData
{
    // 1. 진행도: 유저가 정직하게 뚫고 올라간 최고 스테이지 단계 (0부터 시작)
    // 예: 4라면, 0~4번 인덱스까지 총 5개 스테이지가 해금된 상태
    public int highestUnlockedIndex = 0;

    public List<int> skippedList;
    public int remainedSkipCouponCount;

    // 2. 기록: 각 레벨 ID별 최단 기록 (순서가 바뀌어도 유지됨)
    // Key: LevelID (AssetGUID 또는 레벨명), Value: ClearTime
    // JsonUtility와 Unity Serializer 모두에서 100% 안전한 리스트 구조
    public List<LevelRecord> clearRecords = new List<LevelRecord>();

    // 💡 런타임 조회를 위해 딕셔너리가 필요하다면 헬퍼 메서드로 변환해서 사용
    public float GetClearTime(string levelID)
    {
        var record = clearRecords.Find(r => r.levelID == levelID);
        return record.levelID != null ? record.clearTime : -1f;
    }

    public void SetClearTime(string levelID, float time)
    {
        int index = clearRecords.FindIndex(r => r.levelID == levelID);
        if (index >= 0)
        {
            clearRecords[index] = new LevelRecord(levelID, time);
        }
        else
        {
            clearRecords.Add(new LevelRecord(levelID, time));
        }
    }
}

[System.Serializable]
public struct LevelRecord
{
    public string levelID;
    public float clearTime;

    public LevelRecord(string levelID, float clearTime)
    {
        this.levelID = levelID;
        this.clearTime = clearTime;
    }
}