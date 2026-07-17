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
        Falling // 필요 없을 수도
    }
    private Vector2Int startPos;
    private Vector2Int destPos;

    private MatrixObject mo;

    [SerializeField] MoveState state;
    public MoveState State => state;
    public ObjectPhysicsConfigSO physics;
    void Awake()
    {
        mo = GetComponent<MatrixObject>();
        state = MoveState.Staying;
    }
    
    // 이동 전에 셀 상태를 의도에 맞게 변경해주는 움직이기 전 셋팅을 하는 함수
    public void ExecuteMove(Vector2Int direction, MoveState targetState, bool isAttack = false)
    {
        if (state != MoveState.Staying)
        {
            return;
        }
        
        startPos = new Vector2Int(mo.posX, mo.posY);
        destPos = new Vector2Int(mo.posX + direction.x, mo.posY + direction.y);
        state = targetState;
        
        GamePlayGridManager.Instance.ReserveMove(startPos, destPos, isAttack);
        PerformMove();
    }

    // DOTween과 함께 실제로 움직이고 완료상태를 핸들링해주는 함수
    private void PerformMove()
    {
        if (state == MoveState.Moving)
        {
            Vector3 destWorldPos = GamePlayGridManager.Instance.GetCell(this.destPos.x, this.destPos.y).transform.position;
            transform.DOMove(destWorldPos, physics.moveDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    GamePlayGridManager.Instance.CompleteMove(startPos, destPos);
                    
                    this.state = MoveState.Staying;
                });
        }
    }
}
