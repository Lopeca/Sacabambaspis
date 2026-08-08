using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "GameSettingSO", menuName = "Scriptable Objects/GameSettingSO")]
public class GameSettingSO : ScriptableObject
{
    [SerializeField] private GameObject settingPanelPrefab;
    
    [System.NonSerialized] 
    SettingPanel runtimeInstance;
    public SettingPanel RuntimeInstance => runtimeInstance;

    // 현재 런타임에서 사용할 세팅 데이터
    
    [SerializeField] GameSettingData settingData;
    public GameSettingData SettingData => settingData;
    
    [SerializeField] private AudioMixer audioMixer; 
    [SerializeField] private AudioClip bgmClip;
    public AudioClip BGMClip => bgmClip;
    private const string MASTER_PARAM = "MasterVolume";
    private const string BGM_PARAM = "BgmVolume";
    private const string UI_PARAM = "UIVolume";
    private const string SFX_PARAM = "SfxVolume";

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "game_settings.json");
    private void OnEnable()
    {
        runtimeInstance = null;
    }
    
    public void EnsureUIWarmup()
    {
        if (runtimeInstance == null)
        {
            GameObject go = Instantiate(settingPanelPrefab);
            runtimeInstance = go.GetComponent<SettingPanel>();
            go.SetActive(false);
        }
    }

    public void SaveSettings()
    {
        try
        {
            // 데이터 객체를 JSON 문자열로 직렬화
            string json = JsonUtility.ToJson(SettingData, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[GameSettingSO] 설정 저장 완료: {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameSettingSO] 저장 실패: {e.Message}");
        }
    }

    public void LoadSettings()
    {
        if (File.Exists(SaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                // JSON을 읽어와서 SettingData에 덮어씌움
                settingData = JsonUtility.FromJson<GameSettingData>(json);
                Debug.Log("[GameSettingSO] 설정 로드 완료");
                ApplyAllVolumes();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameSettingSO] 로드 실패: {e.Message}");
                settingData = new GameSettingData(); // 실패 시 기본값 생성
            }
        }
        else
        {
            // 파일이 없으면 기본값으로 저장 파일 생성
            settingData = new GameSettingData();
            SaveSettings();
        }
    }

    public void SetAudioMixerVolume(string parameterName, float linearValue)
    { 
        // 0 이하 값 방지 안전장치
        float clampedValue = Mathf.Max(linearValue, 0.0001f);
        float dB = Mathf.Log10(clampedValue) * 20f;
        
        audioMixer.SetFloat(parameterName, dB);
        
    }

    public void ApplyAllVolumes()
    {
        Debug.Log("모든 볼륨 셋팅 " + settingData.masterVolume);
        SetAudioMixerVolume(MASTER_PARAM, SettingData.masterVolume);
        SetAudioMixerVolume(BGM_PARAM, SettingData.bgmVolume);
        SetAudioMixerVolume(UI_PARAM, SettingData.uiVolume);
        SetAudioMixerVolume(SFX_PARAM, SettingData.sfxVolume);
    }

    public void OpenSettingPanel()
    {
        if(runtimeInstance == null) EnsureUIWarmup();
        runtimeInstance.gameObject.SetActive(true);
    }
}

[Serializable]
public class GameSettingData
{
    // 나중에 설정 항목이 추가되더라도 데이터 클래스만 확장하면 됨
    public float masterVolume = 1.0f;
    public float bgmVolume = 0.8f;
    public float uiVolume = 0.8f;
    public float sfxVolume = 0.8f;
    
    // 예: 추후 추가될 수 있는 항목들
    // public int resolutionIndex = 0;
    // public bool isFullScreen = true;
    // public int targetFrameRate = 60;
}


