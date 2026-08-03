using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "GameSessionSO", menuName = "Scriptable Objects/GameSessionSO")]
public class GameSessionSO : ScriptableObject
{
    public bool isExploringOriginalLevel;
    public int selectedOriginalLevelIndex;  // 게임씬을 왔다갔다 할 때 선택된 스테이지를 유지하기 위함
    public OriginalLevelData selectedOriginalLevelData; // 실제 불러올 레벨 데이터 파일 어드레서블 주소를 참조할 수 있는 필드

    public string selectedCustomLevelPath;

    public LevelSaveData currentLoadedLevelData;    // 실제 레벨 파일 json 안의 데이터
    
    private AsyncOperationHandle<TextAsset> _currentLoadHandle;
    
    /// <summary>
    /// 현재 선택된 맵 조건(오리지널 vs 커스텀)에 따라 LevelSaveData를 채우는 통합 로드 메서드
    /// </summary>
    public async UniTask<bool> LoadSelectedLevelAsync(CancellationToken cancellationToken = default)
    {
        // 이전 어드레서블 로드가 진행 중이었다면 해제
        UnloadCurrentLevelData();

        if (isExploringOriginalLevel)
        {
            if (selectedOriginalLevelData == null || selectedOriginalLevelData.levelAddress == null)
            {
                Debug.LogError("오리지널 레벨 데이터 레퍼런스가 비어있습니다.");
                return false;
            }

            try
            {
                // 1. 어드레서블 핸들 생성 및 비동기 로드
                _currentLoadHandle = selectedOriginalLevelData.levelAddress.LoadAssetAsync<TextAsset>();
                TextAsset jsonAsset = await _currentLoadHandle.ToUniTask(cancellationToken: cancellationToken);

                if (jsonAsset != null)
                {
                    // 2. JSON 파싱
                    currentLoadedLevelData = JsonUtility.FromJson<LevelSaveData>(jsonAsset.text);
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                // 연속 스크롤 등으로 취소되었을 때 처리
                UnloadCurrentLevelData();
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"어드레서블 레벨 데이터 로드 실패: {e.Message}");
                return false;
            }
        }
        else
        {
            // 커스텀 레벨 입출력 스크립트를 통한 로드
           currentLoadedLevelData = CustomLevelFileSystem.LoadLevel(selectedCustomLevelPath);
            return currentLoadedLevelData != null;
        }

        return false;
    }
    
    public void UnloadCurrentLevelData()
    {
        if (_currentLoadHandle.IsValid())
        {
            Addressables.Release(_currentLoadHandle);
        }
        
        currentLoadedLevelData = null;
    }
}
