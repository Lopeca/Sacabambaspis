using System;
using System.Collections;
using _1.Core;
using DG.Tweening;
using UnityEngine;

public class CabbageAI : MonoBehaviour, IGridComponent, IGridInteractable, IGridCreature
{
    private static readonly float durationPerPassiveRotate = 2f;

    private MatrixObject mo;
    GridMovement gridMovement;
    Tween passiveRotateTween;
    private Tween aiRotateTween;

    private Coroutine mainCoroutine;

    private readonly Vector2Int[] directionOrder = { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };
    
    [SerializeField] private bool isBusy;
    [SerializeField] bool passiveRotate; 
    [SerializeField] private int facingDirectionIndex;

    private int rotateIntent;
    
    [SerializeField] private AudioClip playerReactionSFX;

    public bool IsLive { get; set; }

    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
        isBusy = false;
    }

    private void OnEnable()
    {
        mo.OnEliminated += StopAI;
        IsLive = true;
    }

    private void OnDisable()
    {
        mo.OnEliminated -= StopAI;
        IsLive = false;
    }

    private void StopAI()
    {
        IsLive = false;
        passiveRotateTween.Kill();
        aiRotateTween.Kill();
        if (mainCoroutine != null) StopCoroutine(mainCoroutine);
    }

    private void Start()
    {
        if (passiveRotate) StartPassiveRotation();
        mo.AppendGridComponent(this);
    }

    private void StartPassiveRotation()
    {
        passiveRotateTween = transform.DORotate(new Vector3(0, 0, 360f), durationPerPassiveRotate, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }

    // ★ [핵심 1] 외부(FixedUpdate/GamePlayGridManager)의 정식 GridUpdate 턴에만 다음 행동을 결정함
    public void GridUpdate()
    {
        if (isBusy || !gridMovement.IsMoveFinished() || !IsLive) return;
        
        DecideAndExecuteNextAction();
    }
    
    private void DecideAndExecuteNextAction()
    {
        int validDirIndex = ScanThreeDirections();

        if (validDirIndex != -1)
        {
            bool isForward = facingDirectionIndex == validDirIndex;
            facingDirectionIndex = validDirIndex;
            mainCoroutine = StartCoroutine(Co_RotateAndMove(facingDirectionIndex, isForward));
        }
        else
        {
            facingDirectionIndex = AddToFacingDirectionIndex(1); // 좌회전
            mainCoroutine = StartCoroutine(Co_RotateOnly(facingDirectionIndex));
        }
    }

    #region 코루틴 간 바톤 터치

    private IEnumerator Co_RotateAndMove(int targetDirIndex, bool isForward)
    {
        isBusy = true;

        if (!isForward)
        {
            RotateByAI();
            yield return new WaitForSeconds(GameConstants.ENEMY_ROTATE_DURATION);
        }

        if (!IsLive) yield break;

        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + directionOrder[targetDirIndex]);
        if (targetCell.HasPlayer() && targetCell.state == MatrixCell.CellState.Filled)
        {
            mo.ExplodeOnDeath.Explode();
            SoundManager.Instance.PlayUISFX(playerReactionSFX, 1, 1);
            
            yield break;
        }

        // 회전하는 동안(지연 시간) 셀 상태가 변했는지 다시 검사
        if (ScanForwardEmpty())
        {
            gridMovement.ExecuteMove(directionOrder[targetDirIndex], GridMovement.MoveState.Moving, MatrixCell.CellState.Attacking);

            yield return gridMovement.MoveTween.WaitForCompletion();

            // ★ [핵심 2] 코루틴 내부에서 DecideAndExecuteNextAction()을 즉시 재귀 호출하던 로직 제거!
            // isBusy를 해제하여 다음 프레임의 GridUpdate() 턴까지 행동을 유예함.
            isBusy = false;
        }
        else
        {
            isBusy = false;
        }
    }

    private void RotateByAI()
    {
        if (!passiveRotate)
        {
            float angle = 0;
            if (rotateIntent == 1) angle = 90;
            else if (rotateIntent == -1) angle = -90;
            aiRotateTween = transform.DOBlendableLocalRotateBy(new Vector3(0, 0, angle), GameConstants.ENEMY_ROTATE_DURATION, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear);
        }
    }

    private IEnumerator Co_RotateOnly(int newDir)
    {
        isBusy = true;

        RotateByAI();
        yield return new WaitForSeconds(GameConstants.ENEMY_ROTATE_DURATION);

        if (!IsLive) yield break;

        // ★ [핵심 2] 회전 완료 후 즉시 재귀 호출하지 않고 GridUpdate 턴으로 제어권을 넘김
        isBusy = false;
    }

    #endregion

    private int ScanThreeDirections()
    {
        int targetDirectionIndex = AddToFacingDirectionIndex(1);
        if (IsValidCellForMove(targetDirectionIndex)) return targetDirectionIndex;

        targetDirectionIndex = facingDirectionIndex;
        if (IsValidCellForMove(targetDirectionIndex)) return targetDirectionIndex;

        targetDirectionIndex = AddToFacingDirectionIndex(-1);
        if (IsValidCellForMove(targetDirectionIndex)) return targetDirectionIndex;

        return -1;
    }

    // ★ [핵심 3] 이동 가능 셀 판정 강화 (Moving 상태 및 Falling 예정을 절대 선점하지 못하도록 차단)
    private bool IsValidCellForMove(int dirIndex)
    {
        Vector2Int targetDir = directionOrder[dirIndex];
        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + targetDir);

        if (targetCell == null) return false;

        // 플레이어가 이동 중이어서 CellState가 Moving인 칸은 '비어있는 것'으로 취급하지 않음
        bool isCellAvailable = targetCell.state == MatrixCell.CellState.Empty;
        
        bool isPlayerPos = (GamePlayGridManager.Instance.player != null && 
                            targetCell.matrixObject != null && 
                            targetCell.matrixObject == GamePlayGridManager.Instance.player.MO);

        return isCellAvailable || isPlayerPos;
    }

    public bool ScanForwardEmpty()
    {
        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + directionOrder[facingDirectionIndex]);
        
        // 회전 지연 후 최종 진입 직전에도 완벽히 Empty인 상태에서만 진입
        return targetCell != null && targetCell.state == MatrixCell.CellState.Empty;
    }

    private int AddToFacingDirectionIndex(int addValue)
    {
        rotateIntent = addValue;
        int resultValue = facingDirectionIndex + addValue;
        
        if (resultValue >= directionOrder.Length)
            return resultValue - directionOrder.Length;
        if (resultValue < 0)
            return resultValue + directionOrder.Length;
        
        return resultValue;
    }

    private void OnDestroy()
    {
        passiveRotateTween?.Kill(); 
        aiRotateTween?.Kill();
        StopAllCoroutines();
    }

    public void Interact(PlayerController player, Vector2Int direction)
    {
        player.Paralyze();
        SoundManager.Instance.PlayUISFX(playerReactionSFX, 1, 1);
        player.MO.ExplodeOnDeath.Explode();
    }

    public bool Continuous { get; set; }
}