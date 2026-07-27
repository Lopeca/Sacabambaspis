using System;
using System.Collections;
using _1.Core;
using DG.Tweening;
using UnityEngine;

public class CabbageAI : MonoBehaviour, IGridComponent, IGridInteractable
{
    [SerializeField] private float durationPerRotate = 2f; // 1회전(360도)에 걸리는 시간(초)

    private MatrixObject mo;
    GridMovement gridMovement;
    Tween rotateTween;

    private readonly Vector2Int[] directionOrder = { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };
    
    
    [SerializeField] private bool isBusy;
    [SerializeField] bool passiveRotate;  // 양배추는 상시 돌지만, 그렇지 않은 경우도 있을 수 있음
    [SerializeField] private int facingDirectionIndex;

    
    // 디버그용
    [SerializeField] private MatrixCell scannedCell;
    [SerializeField] private int scannedFrameCount;
    private void Awake()
    {
        mo = GetComponent<MatrixObject>();
        gridMovement = GetComponent<GridMovement>();
        isBusy = false;
    }

    private void Start()
    {
        if (passiveRotate) StartSpriteRotation();
        mo.AppendGridComponent(this);
    }

    private void StartSpriteRotation()
    {
        // -360도: Z축 기준 양수는 시계방향, 음수는 반시계방향입니다.
        // SetLoops(-1, LoopType.Incremental): 트윈이 끝날 때마다 위치를 리셋하지 않고 계속 360도씩 더 돌립니다.
        rotateTween = transform.DORotate(new Vector3(0, 0, 360f), durationPerRotate, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }

    public void GridUpdate()
    {
        if (isBusy || !gridMovement.IsMoveFinished()) return;
        
        DecideAndExecuteNextAction();
    }
    
    private void DecideAndExecuteNextAction()
    {
        // 1. 시야 내 3방향(좌 -> 앞 -> 우) 검사
        int validDirIndex = ScanThreeDirections();

        if (validDirIndex != -1)
        {
            // [길 있음]: 회전 후 1칸 이동 코루틴 시작
            // 여기서 바라보는 방향이 이미 바뀜
            bool isForward = facingDirectionIndex == validDirIndex;
            facingDirectionIndex = validDirIndex;
            StartCoroutine(Co_RotateAndMove(facingDirectionIndex, isForward));
        }
        else
        {
            // [길 없음 예외]: 제자리 좌회전 코루틴 시작
            facingDirectionIndex = AddToFacingDirectionIndex(1);   // 좌회전
            StartCoroutine(Co_RotateOnly(facingDirectionIndex));
        }
    }

    #region 코루틴 간 바톤 터치 (주력 업데이트 파이프라인)

    private IEnumerator Co_RotateAndMove(int targetDirIndex, bool isForward)
    {
        isBusy = true;

        //if(passiveRotate)
        if (!isForward)
        {
            yield return new WaitForSeconds(GameConstants.ENEMY_ROTATE_DURATION); // 회전 대기
        }

        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + directionOrder[targetDirIndex]);
        if (targetCell.HasPlayer() && targetCell.state == MatrixCell.CellState.Filled)
        {
            //GamePlayGridManager.Instance.player.PlayerExplode();
            mo.ExplodeOnDeath.Explode();
            yield break;
        }

        // 회전하는 동안 이동예정이었던 셀에 간섭이 생길 수 있어서 한번 더 확인
        if (ScanForwardEmpty())
        {
            gridMovement.ExecuteMove(directionOrder[targetDirIndex], GridMovement.MoveState.Moving,
                MatrixCell.CellState.Attacking);

            yield return gridMovement.MoveTween.WaitForCompletion();

            //isBusy = false;
            yield return null;
            // ★ [핵심] GridUpdate를 기다리지 않고 완료 즉시 다음 동작으로 연속 진행!
            DecideAndExecuteNextAction();
        }
        else
        {
            isBusy = false;
        }
    }

    // newDir은 회전방향이라 히나 만들 때 구현할 예정
    private IEnumerator Co_RotateOnly(int newDir)
    {
        isBusy = true;

        // 제자리 회전 연출이 필요하다면 잠깐 대기 (예: 0.05초 또는 짧은 틱)
        yield return new WaitForSeconds(GameConstants.ENEMY_ROTATE_DURATION);

        isBusy = false;

        // ★ [핵심] 제자리 회전 완료 후 바로 다음 3방향 재검사
        DecideAndExecuteNextAction(); 
    }

    #endregion

    private int ScanThreeDirections()
    {
        Vector2Int targetDir;
        MatrixCell targetCell;

        int targetDirectionIndex = AddToFacingDirectionIndex(1);
        targetDir = directionOrder[targetDirectionIndex];
        targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + targetDir);
        if (targetCell.state == MatrixCell.CellState.Empty || targetCell.state == MatrixCell.CellState.Falling
                                                           //|| (targetCell.HasPlayer() &&targetCell.state == MatrixCell.CellState.Filled))
                                                           || (targetCell.matrixObject != null && targetCell.matrixObject == GamePlayGridManager.Instance.player.MO))
        {
            return targetDirectionIndex;
        }

        // 앞
        targetDirectionIndex = facingDirectionIndex;
        targetDir = directionOrder[targetDirectionIndex];
        targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + targetDir);
        if (targetCell.state == MatrixCell.CellState.Empty || targetCell.state == MatrixCell.CellState.Falling
                                                           || (targetCell.matrixObject != null && targetCell.matrixObject == GamePlayGridManager.Instance.player.MO))

        {
            return targetDirectionIndex;
        }

        // 오른쪽
        targetDirectionIndex = AddToFacingDirectionIndex(-1);
        targetDir = directionOrder[targetDirectionIndex];
        targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + targetDir);
        if (targetCell.state == MatrixCell.CellState.Empty || targetCell.state == MatrixCell.CellState.Falling
                                                           || (targetCell.matrixObject != null && targetCell.matrixObject == GamePlayGridManager.Instance.player.MO))

        {
            return targetDirectionIndex;
        }

        return -1;
    }

    public bool ScanForwardEmpty()
    {
        MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.GetPos() + directionOrder[facingDirectionIndex]);
        //scannedCell = targetCell;
        //scannedFrameCount = Time.frameCount;
        
        return targetCell.state == MatrixCell.CellState.Empty;
    }

    private int AddToFacingDirectionIndex(int addValue)
    {
        int resultValue = facingDirectionIndex + addValue;
        if(resultValue >= directionOrder.Length)
            return resultValue - directionOrder.Length;
        if (resultValue < 0)
            return resultValue + directionOrder.Length;
        return resultValue;
    }

    private void OnDestroy()
    {
        rotateTween?.Kill();
        StopAllCoroutines();
    }

    public void Interact(PlayerController player, Vector2Int direction)
    {
        player.Paralyze();
        player.MO.ExplodeOnDeath.Explode();
    }

    public bool Continuous { get; set; }

}
