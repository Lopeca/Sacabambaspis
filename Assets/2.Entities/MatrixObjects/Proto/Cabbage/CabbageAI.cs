using System;
using DG.Tweening;
using UnityEngine;

public class CabbageAI : MonoBehaviour, IGridComponent, IGridInteractable
{
    [SerializeField] private float durationPerRotate = 2f; // 1회전(360도)에 걸리는 시간(초)

    private MatrixObject mo;
    GridMovement gridMovement;
    Tween rotateTween;

    private float timer;
    private void Start()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
        
        StartRotation();
    }

    private void StartRotation()
    {
        // -360도: Z축 기준 양수는 시계방향, 음수는 반시계방향입니다.
        // SetLoops(-1, LoopType.Incremental): 트윈이 끝날 때마다 위치를 리셋하지 않고 계속 360도씩 더 돌립니다.
        rotateTween = transform.DORotate(new Vector3(0, 0, 360f), durationPerRotate, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }

    public void GridUpdate()
    {
        if (!gridMovement.IsMoveFinished() || timer > 0) return;
        
        
    }

    private void OnDestroy()
    {
        rotateTween?.Kill();
    }

    public void Interact(PlayerController player, Vector2Int direction)
    {
        player.Paralyze();
        player.MO.ExplodeOnDeath.Explode();
    }

    public bool Continuous { get; set; }
}
