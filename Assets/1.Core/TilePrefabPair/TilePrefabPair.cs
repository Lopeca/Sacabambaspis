using System;
using System.Collections.Generic;
using UnityEngine;

public static class TileKeys
{
    public const string Player = "Player";
    public const string Wall = "Wall";
    public const string Zonk = "Zonk";         // 바위
    public const string Infotron = "Infotron"; // 수집 아이템
    public const string Terminal = "Terminal"; // 터미널 오브젝트
    
    public const string WhiteWall = "WhiteWall"; // 터미널 오브젝트
}

// 인스펙터에서 편하게 이름과 프리팹을 매칭하기 위한 구조체
[System.Serializable]
public struct TileDataPair
{
    public string tileKey;       // 에디터에서 입력할 이름 (예: "Player", "Zonk")
    public GameObject prefab;    // 연결할 프리팹 오브젝트
}
public class TilePrefabPair : MonoBehaviour
{
    
    private static TilePrefabPair instance;
    
    public static TilePrefabPair Instance => instance;
    
    [Header("타일 데이터 리스트 (인스펙터 세팅용)")]
    [SerializeField] private List<TileDataPair> tileDataList = new List<TileDataPair>();

    // 실제 코드에서 이름으로 빠르게 프리팹을 찾기 위한 딕셔너리
    private Dictionary<string, GameObject> tileDictionary = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // 💡 만약 누군가 이미 인스턴스를 차지하고 있다면 경고를 띄웁니다.
        if (instance != null && instance != this)
        {
            Debug.LogError($"[TilePrefabPair] 씬에 중복된 싱글톤이 존재합니다! 오브젝트명: {gameObject.name}");
            Destroy(gameObject); // 중복된 녀석은 파괴하거나 리턴
            return;
        }
        instance = this;

        InitDictionary();
    }
    
    private void InitDictionary()
    {
        tileDictionary.Clear();
        foreach (var pair in tileDataList)
        {
            if (string.IsNullOrEmpty(pair.tileKey)) continue;

            if (tileDictionary.ContainsKey(pair.tileKey))
            {
                Debug.LogWarning($"[TilePrefabPair] 중복된 타일 키가 존재합니다: {pair.tileKey}");
                continue;
            }

            tileDictionary.Add(pair.tileKey, pair.prefab);
        }
    }
    
    /// <summary>
    /// 저장된 문자열 키를 통해 해당하는 타일 프리팹을 가져옵니다. (자동완성 및 맵 유지가능)
    /// </summary>
    public GameObject GetPrefab(string key)
    {
        if (tileDictionary.TryGetValue(key, out GameObject prefab))
        {
            return prefab;
        }

        Debug.LogError($"[TilePrefabPair] '{key}'에 해당하는 프리팹을 찾을 수 없습니다!");
        return null;
    }
}
