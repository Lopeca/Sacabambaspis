using UnityEngine;

[CreateAssetMenu(fileName = "MushroomCollectEffect", menuName = "Scriptable Objects/Collectible/MushroomCollectEffect")]
public class MushroomCollectEffect : CollectibleEffect
{
    public override void ApplyEffect()
    {
        GamePlayGridManager.Instance.player.ObtainMushroom();
        base.ApplyEffect();
    }
}
