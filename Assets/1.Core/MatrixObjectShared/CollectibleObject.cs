using System;
using UnityEngine;

public class CollectibleObject : MonoBehaviour, IGridInteractable
{
    private MatrixObject mo;
    TileMaskAnimator tileMaskAnimator;
    public bool Continuous { get; set; }
    public CollectibleEffect collectibleEffect;
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        if (mo == null)
        {
            Debug.LogError("CollectibleObject needs to be attached to this gameobject");
        }
        
        tileMaskAnimator = GetComponent<TileMaskAnimator>();
        if (tileMaskAnimator == null)
        {
            Debug.LogError("No tileMaskAnimator attached");
        }
    }

    public void Interact(PlayerController player, Vector2Int direction)
    {
        // 수집 관련 기능 필요(베이스타일은 수집해도 아무 효과 없는 조건의 수집형 오브젝트)
        tileMaskAnimator.PlayMaskAnimation(direction, ( )=> Destroy(gameObject));
    }


    public void Collect(Vector2Int direction)
    {
        GamePlayGridManager.Instance.ClearCell(mo.GetPos());
        collectibleEffect?.ApplyEffect();
        tileMaskAnimator.PlayMaskAnimation(direction, ( )=> Destroy(gameObject));
    }
}
