using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class CustomLevelManager
{ 
    private const string FolderName = "CustomLevels";
    private const string Extension = ".json";

    private static string FolderPath => Path.Combine(Application.persistentDataPath, FolderName);

    /// <summary>
    /// 저장 폴더가 없으면 생성.
    /// </summary>
    private static void EnsureFolder()
    {
        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);
    }

    /// <summary>
    /// 파일명으로 쓸 수 없는 문자를 제거 (레벨 이름을 그대로 파일명으로 쓸 때 안전장치).
    /// </summary>
    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(clean) ? "Untitled" : clean;
    }
    
    /// <summary>
    /// 레벨 저장. 이미 같은 이름이 있으면 덮어쓴다.
    /// </summary>
    public static bool SaveLevel(LevelSaveData data)
    {
        try
        {
            EnsureFolder();
            string fileName = SanitizeFileName(data.levelName) + Extension;
            string fullPath = Path.Combine(FolderPath, fileName);

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(fullPath, json);

            Debug.Log($"[CustomLevelManager] 저장 완료: {fullPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CustomLevelManager] 저장 실패: {e.Message}");
            return false;
        }
    }
    
    // 레벨 익스플로러가 있다고 전제. 실제로 저장버튼이 있는 곳에 매니저가 없으면 비정상 접근임
    public static bool SaveLevel()
    {
        if (CustomLevelExplorer.Instance == null)
        {
            Debug.Log("비정상 접근 : 레벨 탐색기 관리자가 없어 저장할 수 없습니다.");
            return false;
        }
        
        try
        {
            EnsureFolder();
            string fileName = CustomLevelExplorer.Instance.LoadedLevel.levelName + Extension;
            string fullPath = Path.Combine(CustomLevelExplorer.Instance.GetCurrentFolderPath(), fileName);

            string json = JsonUtility.ToJson(CustomLevelExplorer.Instance.LoadedLevel, true);
            File.WriteAllText(fullPath, json);

            Debug.Log($"[CustomLevelManager] 저장 완료: {fullPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CustomLevelManager] 저장 실패: {e.Message}");
            return false;
        }
    }
    

    /// <summary>
    /// 파일명(확장자 제외)으로 레벨 불러오기.
    /// </summary>
    public static LevelSaveData LoadLevel(string fileNameWithoutExt)
    {
        string fullPath = Path.Combine(FolderPath, fileNameWithoutExt + Extension);

        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[CustomLevelManager] 파일 없음: {fullPath}");
            return null;
        }

        string json = File.ReadAllText(fullPath);
        return JsonUtility.FromJson<LevelSaveData>(json);
    }
    
    /// <summary>
    /// 저장된 커스텀 레벨의 이름(파일명) 목록을 반환.
    /// 커스텀 모드 진입 시 이 리스트로 UI 목록을 채우면 됨.
    /// </summary>
    public static List<string> GetAllLevelNames()
    {
        EnsureFolder();

        return Directory.GetFiles(FolderPath, "*" + Extension)
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(n => n)
            .ToList();
    }

    /// <summary>
    /// 레벨 파일 삭제.
    /// </summary>
    public static bool DeleteLevel(string fileNameWithoutExt)
    {
        string fullPath = Path.Combine(FolderPath, fileNameWithoutExt + Extension);
        if (!File.Exists(fullPath)) return false;

        File.Delete(fullPath);
        return true;
    }
    
    
}
