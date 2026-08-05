using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveRuntimeData", menuName = "Scriptable Objects/Save Runtime Data")]
public class UserRuntimeDataSO : ScriptableObject
{
    [SerializeField] private UserSaveData data = new UserSaveData();
    private Dictionary<string, List<float>> recordDictionary = new Dictionary<string, List<float>>();

    public UserSaveData Data => data;
    private string SavePath => Path.Combine(Application.persistentDataPath, "savefile.json");

    public void Init()
    {
        Load();
    }

    // 레벨 클리어 시 호출
    public void AddRecord(string levelID, float clearTime)
    {
        // 1. 메모리(딕셔너리) 갱신 및 상위 5개 정렬/자르기
        if (!recordDictionary.TryGetValue(levelID, out var timeList))
        {
            timeList = new List<float>();
            recordDictionary.Add(levelID, timeList);
        }
        timeList.Add(clearTime);
        timeList.Sort();
        if (timeList.Count > 5) timeList.RemoveRange(5, timeList.Count - 5);
    }

    // 2. 조회
    public IReadOnlyList<float> GetRecords(string levelID)
    { 
        if (recordDictionary.TryGetValue(levelID, out List<float> records))
        {
            return records;
        }
        return Array.Empty<float>();
    }

    public void Save()
    {
        // JSON 작성을 위해 딕셔너리를 리스트로 갈아치움
        data.clearRecords.Clear();
        foreach (var pair in recordDictionary)
        {
            foreach (var time in pair.Value)
            {
                data.clearRecords.Add(new LevelRecord(pair.Key, time));
            }
        }

        // 파일로 영구 저장
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    // 4. 로드 시점에만 List -> Dictionary 변환
    public void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            data = JsonUtility.FromJson<UserSaveData>(json);
        }
        else
        {
            data = new UserSaveData();
            data.remainedSkipCouponCount = 5;
            Save();
        }

        // 복원 로직
        recordDictionary.Clear();
        foreach (var record in data.clearRecords)
        {
            if (!recordDictionary.TryGetValue(record.levelID, out var timeList))
            {
                timeList = new List<float>();
                recordDictionary.Add(record.levelID, timeList);
            }
            timeList.Add(record.clearTime);
        }

        // 안전을 위해 로드 후 한 번 더 정렬 및 Cut
        foreach (var timeList in recordDictionary.Values)
        {
            timeList.Sort();
            if (timeList.Count > 5) timeList.RemoveRange(5, timeList.Count - 5);
        }
    }

    public void UnlockOriginalStage(int selectedOriginalLevelIndex)
    {
        if (data.highestUnlockedIndex == selectedOriginalLevelIndex)
        {
            data.highestUnlockedIndex++;
        }
        else if (IsSkippedLevel(selectedOriginalLevelIndex))
        {
            data.skippedList.Remove(selectedOriginalLevelIndex);
            data.remainedSkipCouponCount++;
        }
    }

    private bool IsSkippedLevel(int selectedOriginalLevelIndex)
    {
        return data.skippedList.Contains(selectedOriginalLevelIndex);
    }

    // 스킵 버튼은 무조건 스킵 가능한 조건에서 활성화됨. 오류 발생 시 부적절하게 스킵 권한을 쥐어주지 않는지 흐름에서 살피기
    public void SkipLevel()
    {
        data.skippedList.Add(data.highestUnlockedIndex);
        data.highestUnlockedIndex++;
        data.remainedSkipCouponCount--;
        
        Save();
    }
}