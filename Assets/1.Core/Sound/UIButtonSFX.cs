using UnityEngine;
using UnityEngine.UI;

public class UIButtonSFX : MonoBehaviour
{
    [Header("Custom Sound (비워두면 SoundManager 기본 버튼음 재생)")]
    [SerializeField] private AudioClip customClickClip;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // 중복 구독 방지를 위해 Listener 제거 후 등록
        button.onClick.RemoveListener(OnButtonClicked);
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        // 이벤트 해제
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        if (SoundManager.Instance == null) return;

        // 커스텀 클립이 할당되어 있으면 커스텀 소리, 없으면 기본 버튼 sound 출력
        if (customClickClip != null)
        {
            SoundManager.Instance.PlayUISFX(customClickClip);
        }
        else
        {
            SoundManager.Instance.PlayButtonSFX();
        }
    }
}