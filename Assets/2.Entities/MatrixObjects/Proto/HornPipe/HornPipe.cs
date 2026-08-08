using System;
using UnityEngine;

[System.Flags]
public enum PipeDirection
{
    None  = 0,
    Up    = 1 << 0, // 1
    Right = 1 << 1, // 2
    Down  = 1 << 2, // 4
    Left  = 1 << 3  // 8
}
public class HornPipe : MonoBehaviour, IGridInteractable
{
    [SerializeField] private PipeDirection allowedDirections;

    private MatrixObject mo;
    public AudioClip warpSFX;

    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
    }

    public void Interact(PlayerController player, Vector2Int direction)
    {
        // direction은 플레이어가 파이프에 상호작용을 시도한 방향. 즉 플레이어는 상호작용한 방향의 반대편에 있음
        if (!IsDirectionAllowedToEnter(-direction)) return;
        if (!IsOppositeCellEmpty(direction)) return;
        
        // 데이터 적용 후 플레이어 인솔
        PlayerEnter(player, direction);
    }

    private void PlayerEnter(PlayerController player, Vector2Int direction)
    {
        player.Movement.EnterPipe(direction);
        SoundManager.Instance.PlayGameSFX(warpSFX, player.transform.position);
    }

    private bool IsDirectionAllowedToEnter(Vector2Int direction)
    {
        return (allowedDirections & HornPipeUtility.GetDirectionBit(direction)) != 0;
    }

    private bool IsOppositeCellEmpty(Vector2Int direction)
    {
        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + direction);
        return targetCell.state == MatrixCell.CellState.Empty;
    }

    public bool Continuous { get; set; }
}
