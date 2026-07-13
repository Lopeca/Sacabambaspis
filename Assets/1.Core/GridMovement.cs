using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    public enum MoveState
    {
        Staying,
        Moving,
        Rolling,
        Falling
    }
    private Vector2Int startPos;
    private Vector2Int destPos;

    private MatrixObject mo;

    [SerializeField] MoveState state;
    public MoveState State => state;
    private float moveDuration = 0.3f;
    void Awake()
    {
        mo = GetComponent<MatrixObject>();
        state = MoveState.Staying;
    }

    public void RequestMove(Vector2Int direction, MoveState targetState)
    {
        if (state != MoveState.Staying)
        {
            Debug.Log("state ? " + targetState);
            return;
        }
        
        Debug.Log("RequestMove");
        startPos = new Vector2Int(mo.posX, mo.posY);
        destPos = new Vector2Int(mo.posX + direction.x, mo.posY + direction.y);

        if (GamePlayGridManager.Instance.TryReserveMove(mo, startPos, destPos))
        {
            this.state = targetState;

            // 로직 상 움직이기 전에 도착 지점에 먼저 가있는 방식
            // 이를 통해 이동 중 도착지점 위에서 장애물이 떨어지면 도착지점으로 달려드는 유닛과 충돌하여 피해를 입음
            mo.posX = destPos.x;
            mo.posY = destPos.y;
            PerformMove();
        }
    }

    private void PerformMove()
    {
        if (state == MoveState.Moving)
        {
            Vector3 destWorldPos = GamePlayGridManager.Instance.GetCell(this.destPos.x, this.destPos.y).transform.position;
            transform.DOMove(destWorldPos, moveDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    GamePlayGridManager.Instance.CompleteMove(startPos, destPos);
                    
                    this.state = MoveState.Staying;
                });
        }
    }

    // IEnumerator Co_PerformMove()
    // {
    //     // 일단 state = moving 이라고 가정하고 작업
    //     Vector3 targetWorldPos = new Vector3(targetPos.x, targetPos.y, 0); // 2D 그리드 기준
    //     
    //     // 시각적으로 부드럽게 이동하는 도중 (이미 데이터상으로는 출발지와 목적지가 다 잠긴 상태!)
    //     while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
    //     {
    //         transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
    //         yield return null;
    //     }
    //     transform.position = targetWorldPos;
    //     
    //     // 도착 완료 통보
    //     GamePlayGridManager.Instance.CompleteMove(gameObject, startPos, destPos);
    //     
    //     currentPos = targetPos;
    //     isMoving = false;
    // }
}
