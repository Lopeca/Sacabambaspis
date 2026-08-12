using System;
using UnityEngine;

public class CollectibleObject : MonoBehaviour, IGridInteractable
{
    private MatrixObject mo;
    TileMaskAnimator tileMaskAnimator;
    public bool Continuous { get; set; }
    public CollectibleEffect collectibleEffect; // 이건 레거시. 액션쪽이 커스텀 기능을 만들기 좋아보임. 일단은 둘 다 사용
    
    public AudioClip collectSound;
    public AudioClip trapKillSound;

    public bool collected;
    public bool isTrap;
    
    public event Action OnCollected;
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
        if (collected) return;
        collected = true;
        Vector2Int pos = mo.GetPos();
        
        // 수집 관련 기능 필요(베이스타일은 수집해도 아무 효과 없는 조건의 수집형 오브젝트)
        tileMaskAnimator.PlayMaskAnimation(direction, ( )=>
        {
            if(direction == Vector2.zero) GamePlayGridManager.Instance.ClearCell(pos);
            Destroy(gameObject);
        });
    }


    public void Collect(Vector2Int direction)
    {
        collected = true;
        mo.isLive = false;
        SoundManager.Instance.PlayGameSFX(collectSound, transform.position);
        
        if (isTrap)
        {
            SoundManager.Instance.PlayGlobalGameSFX(trapKillSound, 1,1,1,1);
            
            GamePlayGridManager.Instance.player.PlayerExplode();
            return;
        }

        Vector2Int pos = mo.GetPos();
        if (direction != Vector2.zero) GamePlayGridManager.Instance.ClearCell(pos);
        
        collectibleEffect?.ApplyEffect();       // 필드 주석 참고. 액션 사용을 지향해야함
        OnCollected?.Invoke();
        
        tileMaskAnimator.PlayMaskAnimation(direction, ( )=>
        {
            if(direction == Vector2.zero) GamePlayGridManager.Instance.ClearCell(pos);
            Destroy(gameObject);
        });
    }
}
