using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Settings & Mixer")]
    [SerializeField] private GameSettingSO gameSetting;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Prefabs")]
    [SerializeField] private AudioSource sfxAudioSourcePrefab;

    [Header("Pool Config")]
    [SerializeField] private int initialPoolSize = 15;
    [SerializeField] private int maxPoolSize = 30;

    [Header("Audio Clip ShortCut")] 
    [SerializeField] private AudioClip explodeSFX;
    [SerializeField] private AudioClip buttonSFX1;
    [SerializeField] public AudioClip landingSound;
    
    // BGM 전용 오디오 소스 (단일)
    private AudioSource bgmAudioSource;

    // SFX/UI 오디오 소스 풀
    private readonly List<AudioSource> sfxPool = new List<AudioSource>();

    // SFX 클립별 현재 재생 중인 횟수를 트래킹
    private readonly Dictionary<AudioClip, int> activeClipCounts = new Dictionary<AudioClip, int>();

    // 씬 전환 상태 추적 플래그
    private bool isSceneChanging = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBGM();
            InitializeSFXPool();
            gameSetting.LoadSettings();

            // 씬 감지 이벤트 등록
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }
    }

    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        // 씬이 변경되었음을 알리는 일시적 플래그 설정
        isSceneChanging = true;
    
        // 씬 전환 시 재생 중인 3D(공간) 효과음만 즉시 정지 및 풀 반환
        StopAll3DSFX();

        StartCoroutine(ResetSceneChangingFlag());
    }

    private IEnumerator ResetSceneChangingFlag()
    {
        yield return null;
        isSceneChanging = false;
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
        bgmAudioSource.spatialBlend = 0f;
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

    #region SFX / UI Control

    /// <summary>
    /// UI 효과음 전용 (2D, uiGroup 오디오 믹서 출력)
    /// </summary>
    /// <param name="delay">지연 재생 시간(초)</param>
    public void PlayUISFX(AudioClip clip, float volume = 1.0f, float delay = 0f)
    {
        PlaySFXInternal(clip, Vector3.zero, volume, is2D: true, maxSimultaneous: 5, priority: 10, isUI: true, delay: delay);
    }

    /// <summary>
    /// 인게임 2D/3D 공간 효과음 (sfxGroup 오디오 믹서 출력)
    /// </summary>
    /// <param name="position">소리가 나는 월드 좌표</param>
    /// <param name="delay">지연 재생 시간(초)</param>
    public void PlayGameSFX(AudioClip clip, Vector3 position, float volume = 1.0f, int maxSimultaneous = 3, int priority = 128, float delay = 0f)
    {
        PlaySFXInternal(clip, position, volume, is2D: false, maxSimultaneous, priority, isUI: false, delay: delay);
    }
    /// <summary>
    /// 게임 효과음 채널(sfxGroup)을 타지만, 3D 거리 감쇄 없이 공간 제약 없이 출력되는 글로벌 게임 SFX (예: 게임오버 내레이션, 시스템 경고음 등)
    /// </summary>
    public void PlayGlobalGameSFX(AudioClip clip, float volume = 1.0f, int maxSimultaneous = 3, int priority = 128, float delay = 0f)
    {
        // position은 Vector3.zero로 넘기되 is2D = true로 설정하여 spatialBlend를 0으로 만듭니다.
        PlaySFXInternal(clip, Vector3.zero, volume, is2D: true, maxSimultaneous, priority, isUI: false, delay: delay);
    }

    private void PlaySFXInternal(AudioClip clip, Vector3 position, float volume, bool is2D, int maxSimultaneous, int priority, bool isUI, float delay = 0f)
    {
        if (clip == null) return;

        // 지연 시간이 설정된 경우 코루틴으로 대기 후 처리
        if (delay > 0f)
        {
            StartCoroutine(Routine_PlaySFXWithDelay(clip, position, volume, is2D, maxSimultaneous, priority, isUI, delay));
            return;
        }

        ExecutePlaySFX(clip, position, volume, is2D, maxSimultaneous, priority, isUI);
    }

    private IEnumerator Routine_PlaySFXWithDelay(AudioClip clip, Vector3 position, float volume, bool is2D, int maxSimultaneous, int priority, bool isUI, float delay)
    {
        int initialSceneIndex = SceneManager.GetActiveScene().buildIndex;

        yield return new WaitForSeconds(delay);

        // 지연 시간 동안 씬이 바뀌었는지 체크
        if (!is2D && (SceneManager.GetActiveScene().buildIndex != initialSceneIndex || isSceneChanging))
        {
            Debug.LogWarning($"[SoundManager] InGame SFX '{clip.name}' 재생 취소됨: 지연 시간 도중 씬이 변경되었습니다.");
            yield break;
        }

        ExecutePlaySFX(clip, position, volume, is2D, maxSimultaneous, priority, isUI);
    }

    private void ExecutePlaySFX(AudioClip clip, Vector3 position, float volume, bool is2D, int maxSimultaneous, int priority, bool isUI)
    {
        // 1. 동시 재생 제한 체크
        if (activeClipCounts.TryGetValue(clip, out int currentCount))
        {
            if (currentCount >= maxSimultaneous) return;
        }

        // 2. 오디오 소스 할당
        AudioSource source = GetAvailableAudioSource();
        if (source == null) return;

        // 3. 트래킹 카운트 증가
        if (!activeClipCounts.ContainsKey(clip)) activeClipCounts[clip] = 0;
        activeClipCounts[clip]++;

        // 4. 오디오 설정 (UI / SFX 오디오 믹서 채널 분기)
        source.outputAudioMixerGroup = isUI ? uiGroup : sfxGroup;
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = is2D ? 0f : 1f;
        source.priority = priority;
        source.gameObject.SetActive(true);
        source.Play();

        // 5. 반환 코루틴
        StartCoroutine(Routine_ReturnToPool(source, clip));
    }

    private IEnumerator Routine_ReturnToPool(AudioSource source, AudioClip clip)
    {
        yield return new WaitForSeconds(clip.length);

        source.Stop();
        source.gameObject.SetActive(false);

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

        if (sfxPool.Count < maxPoolSize)
        {
            AudioSource newSource = CreateNewPooledAudioSource();
            return newSource;
        }

        return null;
    }

    #endregion

    #region Shortcut Methods
    
    // UI 버튼 수동 연결용 기본 클릭음 숏컷
    public void PlayButtonSFX()
    {
        PlayUISFX(buttonSFX1);
    }

    public void PlayExplodeSFX(Vector3 transformPosition, float delay = 0f)
    {
        PlayGameSFX(explodeSFX, transformPosition, 0.3f, delay: delay);
    }

    #endregion
    
    /// <summary>
    /// 씬 전환 시 부자연스러운 공간 음향 현상을 방지하기 위해 재생 중인 3D 효과음만 즉시 정지합니다.
    /// </summary>
    private void StopAll3DSFX()
    {
        for (int i = 0; i < sfxPool.Count; i++)
        {
            AudioSource source = sfxPool[i];

            // 활성화된 오디오 소스 중 3D 사운드(spatialBlend > 0)인 것만 정지 및 비활성화
            if (source.gameObject.activeSelf && source.spatialBlend > 0f)
            {
                // 현재 플레이 중인 클립 가져오기
                AudioClip clip = source.clip;

                source.Stop();
                source.gameObject.SetActive(false);

                // 트래킹 카운터 차감
                if (clip != null && activeClipCounts.ContainsKey(clip))
                {
                    activeClipCounts[clip]--;
                    if (activeClipCounts[clip] <= 0)
                    {
                        activeClipCounts.Remove(clip);
                    }
                }
            }
        }
    }
}