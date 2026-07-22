using UnityEngine;

[CreateAssetMenu(fileName = "ChickenCollectEffect", menuName = "Scriptable Objects/Collectible/ChickenCollectEffect")]
public class ChickenCollectEffect : CollectibleEffect
{
    public override void ApplyEffect()
    {
        if (GamePlayGridManager.Instance == null) return;
        
    }
}
