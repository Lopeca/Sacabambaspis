using System;
using UnityEngine;

public class MushroomItem : MonoBehaviour
{
    [SerializeField] Mushroom mushroom;
    [SerializeField] CollectibleObject collectible;

    private PlayerController Player => GamePlayGridManager.Instance.player;

    private void Awake()
    {
        mushroom.gameObject.SetActive(false);
        mushroom.Init();
    }

    private void OnEnable()
    {
        collectible.OnCollected += GiveMushroomToPlayer;
    }

    private void OnDisable()
    {
        collectible.OnCollected -= GiveMushroomToPlayer;
    }

    private void GiveMushroomToPlayer()
    {
        Player.GetMushroom(mushroom);
    }
}
