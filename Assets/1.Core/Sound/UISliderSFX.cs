using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UISliderSFX : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum SoundChannelType
    {
        UI,         // UI 채널 (PlayUISFX)
        GameSFX,    // SFX 채널 (PlayGameSFX - 2D/3D 글로벌)
        GlobalGameSFX // SFX 채널 (PlayGlobalGameSFX)
    }

    [Header("Click SFX (눌렀을 때)")]
    [SerializeField] private AudioClip customClickClip; // 비워두면 기본 버튼음

    [Header("Test SFX (손 뗐을 때 테스트)")]
    [SerializeField] private SoundChannelType channelType = SoundChannelType.GameSFX;
    [SerializeField] private AudioClip testSFXClip;     // 테스트할 효과음 클립

    public void OnPointerDown(PointerEventData eventData)
    {
        if (SoundManager.Instance == null) return;

        // 터치/클릭 시 반응음
        if (customClickClip != null)
            SoundManager.Instance.PlayUISFX(customClickClip);
        else
            SoundManager.Instance.PlayButtonSFX();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (SoundManager.Instance == null || testSFXClip == null) return;

        // 지정한 채널 타입에 맞춰 테스트 사운드 1회 재생
        switch (channelType)
        {
            case SoundChannelType.UI:
                SoundManager.Instance.PlayUISFX(testSFXClip);
                break;

            case SoundChannelType.GameSFX:
                // transform.position을 넘기되, 2D 테스트음처럼 원음 크기대로 들리도록 글로벌 위치로 전달
                SoundManager.Instance.PlayGameSFX(testSFXClip, transform.position);
                break;

            case SoundChannelType.GlobalGameSFX:
                SoundManager.Instance.PlayGlobalGameSFX(testSFXClip);
                break;
        }
    }
}