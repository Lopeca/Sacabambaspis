using UnityEngine;

public class ScreenModeController : MonoBehaviour
{
    // 16:9 추천 창모드 해상도
    private const int WindowedWidth = 1600;
    private const int WindowedHeight = 900;

    public void ToggleFullScreen()
    {
        if (Screen.fullScreen)
        {
            // 전체화면 -> 1600x900 창모드로 전환
            Screen.SetResolution(WindowedWidth, WindowedHeight, FullScreenMode.Windowed);
        }
        else
        {
            // 창모드 -> 전체화면으로 전환 (모니터 최대 해상도 자동 맞춤)
            Resolution maxRes = Screen.currentResolution;
            Screen.SetResolution(maxRes.width, maxRes.height, FullScreenMode.FullScreenWindow);
        }
    }

    /// <summary>
    /// 특정 해상도 지정 + 창모드 전환 (선택 사항)
    /// 예: 1280x720 창모드로 변경하고 싶을 때 사용
    /// </summary>
    public void SetWindowedMode(int width = 1280, int height = 720)
    {
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
    }

    /// <summary>
    /// 전체화면으로 변경
    /// </summary>
    public void SetFullScreen()
    {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; // 또는 FullScreenWindow
    }
}
