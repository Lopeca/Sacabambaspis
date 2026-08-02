using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "GameSessionSO", menuName = "Scriptable Objects/GameSessionSO")]
public class GameSessionSO : ScriptableObject
{
    public bool isExploringOriginalLevel;
    public int selectedOriginalLevelIndex;  // 게임씬을 왔다갔다 할 때 선택된 스테이지를 유지하기 위함
    public OriginalLevelData selectedOriginalLevelData; // 실제 불러올 레벨 데이터 파일 어드레서블 주소를 참조할 수 있는 필드
    
    public string selectedCustomLevelPath;
}
