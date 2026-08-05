using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Settings & Mixer")]
    [SerializeField] private GameSettingSO gameSetting;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Prefabs")]
    [SerializeField] private AudioSource sfxAudioSourcePrefab;

    [Header("Pool Config")]
    [SerializeField] private int initialPoolSize = 15;
    [SerializeField] private int maxPoolSize = 30;

    // BGM 전용 오디오 소스 (단일)
    private AudioSource bgmAudioSource;

    // SFX 오디오 소스 풀
    private readonly List<AudioSource> sfxPool = new List<AudioSource>();

    // SFX 클립별 현재 재생 중인 횟수를 트래킹 (폭발음 연속 묻힘 방지)
    private readonly Dictionary<AudioClip, int> activeClipCounts = new Dictionary<AudioClip, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBGM();
            InitializeSFXPool();
            gameSetting.LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        gameSetting.ApplyAllVolumes();
        PlayBGM(gameSetting.BGMClip);
    }

    private void InitializeBGM()
    {
        GameObject bgmGO = new GameObject("BGM_AudioSource");
        bgmGO.transform.SetParent(transform);
        bgmAudioSource = bgmGO.AddComponent<AudioSource>();
        bgmAudioSource.outputAudioMixerGroup = bgmGroup;
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.spatialBlend = 0f; // BGM은 완전 2D
    }

    private void InitializeSFXPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPooledAudioSource();
        }
    }

    private AudioSource CreateNewPooledAudioSource()
    {
        AudioSource newSource = Instantiate(sfxAudioSourcePrefab, transform);
        newSource.gameObject.SetActive(false);
        sfxPool.Add(newSource);
        return newSource;
    }

    #region BGM Control
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        
        bgmAudioSource.clip = clip;
        bgmAudioSource.Play();
    }

    public void StopBGM()
    {
        bgmAudioSource.Stop();
    }
    #endregion

    #region SFX Control

    /// <summary>
    /// UI 효과음 전용 (2D, 위치 상관없이 출력)
    /// </summary>
    public void PlayUISFX(AudioClip clip, float volume = 1.0f)
    {
        PlaySFXInternal(clip, Vector3.zero, volume, is2D: true, maxSimultaneous: 5, priority: 10);
    }

    /// <summary>
    /// 인게임 2D 공간 효과음 (플레이어와의 거리에 따른 감쇄 적용)
    /// </summary>
    /// <param name="position">소리가 나는 월드 좌표</param>
    /// <param name="maxSimultaneous">해당 오디오 클립의 동시 재생 제한 수 (예: 폭발음은 3개)</param>
    /// <param name="priority">0(최우선) ~ 256(최하위), 기본값 128</param>
    public void PlayInGameSFX(AudioClip clip, Vector3 position, float volume = 1.0f, int maxSimultaneous = 3, int priority = 128)
    {
        PlaySFXInternal(clip, position, volume, is2D: false, maxSimultaneous, priority);
    }

    private void PlaySFXInternal(AudioClip clip, Vector3 position, float volume, bool is2D, int maxSimultaneous, int priority)
    {
        if (clip == null) return;

        // 1. 해당 클립의 동시 재생 수 제한 체크 (폭발음 묻힘 및 도배 방지)
        if (activeClipCounts.TryGetValue(clip, out int currentCount))
        {
            if (currentCount >= maxSimultaneous)
            {
                // 설정한 동시 재생 한도를 초과하면 재생 요청을 무시하여 중요한 소리를 보존
                return;
            }
        }

        // 2. 풀에서 사용 가능한 AudioSource 가져오기
        AudioSource source = GetAvailableAudioSource();
        if (source == null) return;

        // 3. 트래킹 횟수 증가
        if (!activeClipCounts.ContainsKey(clip)) activeClipCounts[clip] = 0;
        activeClipCounts[clip]++;

        // 4. 오디오 소스 설정 및 재생
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = is2D ? 0f : 1f; // UI는 2D(0), 인게임은 3D(1)
        source.priority = priority;
        source.gameObject.SetActive(true);
        source.Play();

        // 5. 재생이 끝나면 풀로 반환하는 코루틴/Delay 처리 대신 LeanCheck 사용
        StartCoroutine(Routine_ReturnToPool(source, clip));
    }

    private System.Collections.IEnumerator Routine_ReturnToPool(AudioSource source, AudioClip clip)
    {
        // 클립 길이만큼 대기
        yield return new WaitForSeconds(clip.length);

        source.Stop();
        source.gameObject.SetActive(false);

        // 카운트 차감
        if (activeClipCounts.ContainsKey(clip))
        {
            activeClipCounts[clip]--;
            if (activeClipCounts[clip] <= 0)
            {
                activeClipCounts.Remove(clip);
            }
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        for (int i = 0; i < sfxPool.Count; i++)
        {
            if (!sfxPool[i].gameObject.activeSelf)
            {
                return sfxPool[i];
            }
        }

        // 풀에 여유가 없고 maxPoolSize보다 작으면 추가 확장
        if (sfxPool.Count < maxPoolSize)
        {
            AudioSource newSource = CreateNewPooledAudioSource();
            return newSource;
        }

        // 풀이 가득 찼으면 Priority가 낮거나 오래된 소리를 재활용하거나 null 반환
        return null;
    }

    #endregion
}