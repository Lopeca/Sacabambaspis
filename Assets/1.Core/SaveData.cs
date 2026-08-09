using System;
using System.Collections.Generic;
using TMPro;
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

    // 2. 기록: 각 레벨 ID별 최단 기록 (순서가 바뀌어도 유지됨)
    // Key: LevelID (AssetGUID 또는 레벨명), Value: ClearTime
    // JsonUtility와 Unity Serializer 모두에서 100% 안전한 리스트 구조
    public List<LevelRecord> clearRecords = new List<LevelRecord>();

    public LevelState GetLevelState(int targetIndex)
    {
        // 1. 최고 해금 인덱스보다 뒤에 있는 레벨은 무조건 잠김
        if (targetIndex > highestUnlockedIndex)
        {
            return LevelState.Locked;
        }

        // 2. 스킵한 리스트에 들어있는 경우
        else if (skippedList != null && skippedList.Contains(targetIndex))
        {
            return LevelState.Skipped;
        }
        
        else if (targetIndex == highestUnlockedIndex)
        {
            return LevelState.Unlocked;
        }

        return LevelState.Cleared;
    }

    public Dictionary<string, List<float>> ConvertRecordListToDictionary()
    {
        Dictionary<string, List<float>> dictionary = new Dictionary<string, List<float>>();

        foreach (var clearRecord in clearRecords)
        {
            if (!dictionary.TryGetValue(clearRecord.levelID, out List<float> timeList))
            {
                timeList = new List<float>();
                dictionary.Add(clearRecord.levelID, timeList);
            }

            timeList.Add(clearRecord.clearTime);
        }

        // 각 레벨별 기록 정렬 및 상위 5개 자르기
        foreach (var timeList in dictionary.Values)
        {
            timeList.Sort(); // 오름차순 정렬 (빠른 기록이 앞으로)

            if (timeList.Count > 5)
            {
                timeList.RemoveRange(5, timeList.Count - 5); // 5위 이후 기록 제거
            }
        }

        return dictionary;
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

public enum LevelState
{
    Locked, // 잠김 (진행 불가)
    Unlocked, // 해금됨 (아직 클리어 안 함 / 도전 가능)
    Cleared, // 클리어함
    Skipped // 스킵권으로 넘어감 (나중에 다시 클리어 가능)
}

public static class LevelStateExtensions
{
    public static string ToString(this LevelState state)
    {
        return state switch
        {
            LevelState.Locked => "잠김",
            LevelState.Unlocked => "열림",
            LevelState.Cleared => "성공",
            LevelState.Skipped => "건너뜀",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
    
    public static Color ToColor(this LevelState state)
    {
        return state switch
        {
            LevelState.Locked => Color.gray,
            LevelState.Unlocked => Color.dodgerBlue,
            LevelState.Cleared => Color.aquamarine,
            LevelState.Skipped => Color.mediumVioletRed,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}