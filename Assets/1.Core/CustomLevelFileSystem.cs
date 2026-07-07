using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


/// <summary>
/// 커스텀 레벨을 폴더 구조로 관리.
/// 실제 디스크 폴더 구조를 그대로 트리로 스캔해서 보여주고,
/// 폴더 생성/이동/삭제 같은 기본 오퍼레이션 제공.
/// </summary>
public static class CustomLevelFileSystem
{
    private const string Extension = ".json";

    public static string RootPath => Path.Combine(Application.persistentDataPath, "CustomLevels");

    public static void EnsureRoot()
    {
        if (!Directory.Exists(RootPath))
            Directory.CreateDirectory(RootPath);
    }

    // ---------- 트리 구조 ----------

    public class LevelFolderNode
    {
        public string FolderName;
        public string FullPath;
        public List<LevelFolderNode> SubFolders = new List<LevelFolderNode>();
        public List<string> LevelFiles = new List<string>(); // 확장자 제외 파일명
    }

    /// <summary>
    /// RootPath부터 재귀적으로 폴더 트리를 스캔.
    /// 게임 안 브라우저 UI에서 이 트리로 렌더링하면 됨.
    /// </summary>
    public static LevelFolderNode ScanTree(string path = null)
    {
        EnsureRoot();
        path ??= RootPath;

        var node = new LevelFolderNode
        {
            FolderName = Path.GetFileName(path),
            FullPath = path
        };

        foreach (var dir in Directory.GetDirectories(path))
            node.SubFolders.Add(ScanTree(dir));

        foreach (var file in Directory.GetFiles(path, "*" + Extension))
            node.LevelFiles.Add(Path.GetFileNameWithoutExtension(file));

        return node;
    }

    // ---------- 폴더 오퍼레이션 ----------

    public static bool CreateFolder(string parentFolderFullPath, string newFolderName)
    {
        try
        {
            string newPath = Path.Combine(parentFolderFullPath, SanitizeName(newFolderName));
            if (Directory.Exists(newPath)) return false;
            Directory.CreateDirectory(newPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CustomLevelFileSystem] 폴더 생성 실패: {e.Message}");
            return false;
        }
    }

    public static bool RenameFolder(string folderFullPath, string newName)
    {
        try
        {
            string parent = Directory.GetParent(folderFullPath).FullName;
            string newPath = Path.Combine(parent, SanitizeName(newName));
            Directory.Move(folderFullPath, newPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CustomLevelFileSystem] 폴더 이름 변경 실패: {e.Message}");
            return false;
        }
    }

    public static bool DeleteFolder(string folderFullPath, bool recursive = true)
    {
        try
        {
            if (folderFullPath == RootPath) return false; // 루트는 삭제 금지
            Directory.Delete(folderFullPath, recursive);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CustomLevelFileSystem] 폴더 삭제 실패: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 폴더 자체를 다른 폴더 하위로 이동 (하위 폴더/파일 포함 전체 이동).
    /// </summary>
    public static bool MoveFolder(string sourceFolderFullPath, string destParentFolderFullPath)
    {
        try
        {
            string folderName = Path.GetFileName(sourceFolderFullPath);
            string destPath = Path.Combine(destParentFolderFullPath, folderName);

            if (Directory.Exists(destPath)) return false;
            if (destPath.StartsWith(sourceFolderFullPath)) return false; // 자기 자신 하위로 이동 방지

            Directory.Move(sourceFolderFullPath, destPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CustomLevelFileSystem] 폴더 이동 실패: {e.Message}");
            return false;
        }
    }

    // ---------- 레벨 파일 오퍼레이션 ----------

    public static bool SaveLevel(LevelSaveData data, string targetFolderFullPath)
    {
        try
        {
            if (!Directory.Exists(targetFolderFullPath))
                Directory.CreateDirectory(targetFolderFullPath);

            string fileName = SanitizeName(data.levelName) + Extension;
            string fullPath = Path.Combine(targetFolderFullPath, fileName);
            File.WriteAllText(fullPath, JsonUtility.ToJson(data, true));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CustomLevelFileSystem] 저장 실패: {e.Message}");
            return false;
        }
    }

    public static LevelSaveData LoadLevel(string levelFileFullPathWithoutExt)
    {
        string fullPath = levelFileFullPathWithoutExt + Extension;
        if (!File.Exists(fullPath)) return null;
        return JsonUtility.FromJson<LevelSaveData>(File.ReadAllText(fullPath));
    }

    /// <summary>
    /// 레벨 파일을 다른 폴더로 이동. "최상위에 만든 걸 원하는 폴더로 옮기기" 케이스가 이거.
    /// </summary>
    public static bool MoveLevel(string sourceFileFullPathWithoutExt, string destFolderFullPath)
    {
        try
        {
            string sourcePath = sourceFileFullPathWithoutExt + Extension;
            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(destFolderFullPath, fileName);

            if (File.Exists(destPath)) return false;

            File.Move(sourcePath, destPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CustomLevelFileSystem] 레벨 이동 실패: {e.Message}");
            return false;
        }
    }

    public static bool DeleteLevel(string levelFileFullPathWithoutExt)
    {
        string fullPath = levelFileFullPathWithoutExt + Extension;
        if (!File.Exists(fullPath)) return false;
        File.Delete(fullPath);
        return true;
    }

    private static string SanitizeName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(clean) ? "Untitled" : clean;
    }
}