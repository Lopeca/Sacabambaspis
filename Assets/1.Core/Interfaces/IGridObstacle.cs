using UnityEngine;

public interface IGridObstacle
{
    /// <summary>
    /// 플레이어가 이 오브젝트가 있는 칸으로 통과(진입)를 시도합니다.
    /// </summary>
    /// <param name="player">진입을 시도하는 플레이어</param>
    /// <param name="direction">진입하려는 방향</param>
    /// <returns>통과하여 이 칸을 꿰찰 수 있으면 true, 막혀서 못 가면 false</returns>
    bool TryPassThrough(PlayerController player, Vector2Int direction);
}
