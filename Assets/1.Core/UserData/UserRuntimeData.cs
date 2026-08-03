using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveRuntimeData", menuName = "Scriptable Objects/Save Runtime Data")]
public class UserRuntimeData : ScriptableObject
{
    // 1. 인스펙터 노출용 필드
    [SerializeField] private UserSaveData data = new UserSaveData();

    // 2. 외부 읽기 전용 프로퍼티
    public UserSaveData Data => data;

    // ⭕ [안전] SavePath를 부르는 '그 순간(런타임)'에 경로를 생성하도록 프로퍼티로 작성
    private string SavePath => Path.Combine(Application.persistentDataPath, "savefile.json");

    public void Init()
    {
        Load();
    }
    public void Save()
    {
        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(SavePath, json);
    }

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
    }
}