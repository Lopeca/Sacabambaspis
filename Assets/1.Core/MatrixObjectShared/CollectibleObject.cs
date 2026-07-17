using System;
using UnityEngine;

public class CollectibleObject : MonoBehaviour, IGridObstacle
{
    private MatrixObject owner;
    TileMaskAnimator tileMaskAnimator;
    private void Awake()
    {
        owner = GetComponent<MatrixObject>();
        if (owner == null)
        {
            Debug.LogError("CollectibleObject needs to be attached to this gameobject");
        }
        
        tileMaskAnimator = GetComponent<TileMaskAnimator>();
        if (tileMaskAnimator == null)
        {
            Debug.LogError("No tileMaskAnimator attached");
        }
    }

    public bool TryInteract(PlayerController player, Vector2Int direction)
    {
        // 수집 관련 기능 필요(베이스타일은 수집해도 아무 효과 없는 조건의 수집형 오브젝트)
        tileMaskAnimator.PlayMaskAnimation(direction, ( )=> Destroy(gameObject));
        return true;
    }

    public void Collect(Vector2Int direction)
    {
        tileMaskAnimator.PlayMaskAnimation(direction, ( )=> Destroy(gameObject));
    }
}
