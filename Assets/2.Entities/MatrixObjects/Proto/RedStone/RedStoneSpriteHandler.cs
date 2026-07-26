using System;
using UnityEngine;

public class RedStoneSpriteHandler : MonoBehaviour
{
    [SerializeField] private Sprite idle;
    [SerializeField] private Sprite falling;
    
    SpriteRenderer spriteRenderer;
    GridGravity gravity;

    private void Awake()
    {
        gravity = GetComponent<GridGravity>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // ★ 람다식을 이용해 매개변수를 묶어서 넘겨줍니다.
        if (gravity != null)
        {
            gravity.OnStartFalling += OnStartFallingHandler;
            gravity.OnEndFalling += OnEndFallingHandler;
        }
    }

    private void OnDisable()
    {
        // ★ 메모리 누수 방지를 위해 해제해줍니다.
        if (gravity != null)
        {
            gravity.OnStartFalling -= OnStartFallingHandler;
            gravity.OnEndFalling -= OnEndFallingHandler;
        }
    }

    private void OnStartFallingHandler() => SetSprite(falling);
    private void OnEndFallingHandler() => SetSprite(idle);

    private void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}
