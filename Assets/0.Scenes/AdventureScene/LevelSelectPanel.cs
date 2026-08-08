using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectPanel : MonoBehaviour
{
    [Header("Save Data & References")]
    [SerializeField] private UserRuntimeDataSO userDataSo;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject contentBox;
    [SerializeField] private GameObject levelSelectButtonPrefab;
    [SerializeField] private GameSessionSO gameSession;

    [Header("State")]
    [SerializeField] private LevelSelectButton currentSelectedButton;
    [SerializeField] private int currentSelectedButtonIndex;
    public LevelSelectButton CurrentSelectedButton => currentSelectedButton;

    private List<LevelSelectButton> levelSelectButtons;
    private Coroutine scrollCoroutine;

    private void Awake()
    {
        levelSelectButtons = new List<LevelSelectButton>();
    }

    public void Init(OriginalLevelDatabase levelDB)
    {
        // 1. SO 데이터 로드는 최우선으로 진행(이 초기화 함수 나중에 게임 첫 로딩 수준의 영역으로 넘겨둘 것)
        userDataSo.Init();

        contentBox.ClearChildren();
        levelSelectButtons.Clear(); 

        int index = 0;
        foreach (OriginalLevelData levelData in levelDB.originalLevels)
        {
            LevelSelectButton button = Instantiate(levelSelectButtonPrefab, contentBox.transform).GetComponent<LevelSelectButton>();
            
            button.Init(index, levelData);
            index++;
            
            levelSelectButtons.Add(button);
        }
        
        Debug.Log(userDataSo.Data.highestUnlockedIndex + ":: " + levelDB.originalLevels.Count);
        
        
        // 포커싱할 레벨 조율

        int targetIndex;
        if (gameSession.gameSceneEnded)
        {
            gameSession.gameSceneEnded = false;
            targetIndex = gameSession.selectedOriginalLevelIndex;
        }
        else
        {
            if (userDataSo.Data.highestUnlockedIndex >= levelDB.originalLevels.Count)
                targetIndex = levelDB.originalLevels.Count - 1;
            else
                targetIndex = userDataSo.Data.highestUnlockedIndex;
        }

        Debug.Log(targetIndex+ "<=targetIndex");
        currentSelectedButton = levelSelectButtons[targetIndex];
        currentSelectedButton.OnClick();
    }

    public void FocusButton(int index)
    {
        if (index < 0 || index >= levelSelectButtons.Count) return;
        
        if (currentSelectedButton != null)
            currentSelectedButton.DeSelect();

        currentSelectedButton = levelSelectButtons[index];
        currentSelectedButtonIndex = index;
        currentSelectedButton.Select();

        // 💡 [수정] 스크롤 이동을 한 프레임 뒤로 미루어서 완벽한 UI 레이아웃 좌표를 보장받음
        if (scrollCoroutine != null)
            StopCoroutine(scrollCoroutine);

        scrollCoroutine = StartCoroutine(Co_ScrollNextFrame(currentSelectedButton.GetComponent<RectTransform>()));
    }

    // 💡 레이아웃 계산 완성을 위해 1프레임 대기하는 코루틴
    private IEnumerator Co_ScrollNextFrame(RectTransform targetRect)
    {
        // 유니티 Canvas 레이아웃 갱신 강제
        Canvas.ForceUpdateCanvases();
        
        // UI Layout(ContentSizeFitter) 계산이 완료되도록 1프레임 대기
        yield return null;
        
        Canvas.ForceUpdateCanvases();

        // 이제 완벽히 계산된 좌표로 스크롤 계산 시작
        ScrollToButtonImmediate(targetRect);
    }

    private void ScrollToButtonImmediate(RectTransform targetRect)
    {
        if (scrollRect == null || targetRect == null) return;

        RectTransform contentRect = scrollRect.content;
        RectTransform viewportRect = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;

        float scrollableHeight = contentRect.rect.height - viewportRect.rect.height;

        if (scrollableHeight <= 0f) return;

        float targetY = Mathf.Abs(targetRect.anchoredPosition.y);
        float targetCenterOffset = targetY - (viewportRect.rect.height * 0.5f) + (targetRect.rect.height * 0.5f);

        float targetNormalizedPos = 1.0f - Mathf.Clamp01(targetCenterOffset / scrollableHeight);

        // 스크롤 부드럽게 이동
        StartCoroutine(Co_SmoothScroll(targetNormalizedPos, 0.2f));
    }

    private IEnumerator Co_SmoothScroll(float targetPos, float duration)
    {
        float elapsed = 0f;
        float startPos = scrollRect.verticalNormalizedPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = targetPos;
    }

    public void SelectNextLevelButton()
    {
        if (currentSelectedButtonIndex + 1 >= levelSelectButtons.Count) return;
        currentSelectedButtonIndex++;
        levelSelectButtons[currentSelectedButtonIndex].OnClick();
    }

}
