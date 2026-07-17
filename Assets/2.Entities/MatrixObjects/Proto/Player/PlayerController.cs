using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Ready,      // 입력을 받고 수행할 수 있는 상태
        Moving,     // 입력 수행 중인 상태
        Frozen,     // 에디터나, 일시정지 등
        Dead        // 사망. Frozen에 흡수될 가능성 있음
    }
    
    private double lastXInputTime;
    private double lastYInputTime;
    private Vector2Int moveInput;

    [SerializeField]private PlayerState state;
    public PlayerState State => state;

    private MatrixObject mo;
    public MatrixObject MO => mo;
    GridMovement movement;
    private void Awake()
    {
        state = PlayerState.Frozen;
        mo = GetComponent<MatrixObject>();
        movement = GetComponent<GridMovement>();

    }

    private void Start()
    {
        if(GamePlayGridManager.Instance.player == null)
            GamePlayGridManager.Instance.player = this;
    }

    public void PlayerUpdate()
    {
        if (state == PlayerState.Ready)
        {
            HandleInput();
        }
        else if (state == PlayerState.Moving)
        {
            if (movement.State == GridMovement.MoveState.Staying)
                state = PlayerState.Ready;
        }
    }

    private void HandleInput()
    {
        if (moveInput != Vector2.zero)
        {
            // 나중에 무턱대고 요청이 아니라 움직일 수 있는지 여기서 확인하고 움직이는 식으로 바꾸기
            // 다른 오브젝트들은 조건을 보고 틀리면 다른 선택을 해야해서 이 요청 함수 안에 들어가서 이동 가능한지 검사하고 이동까지 다 하면 모듈화가 꼬임
             MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.posX + moveInput.x, mo.posY + moveInput.y);
            
            if (IsDestinationEmpty(targetCell))
            {
                movement.ExecuteMove(moveInput, GridMovement.MoveState.Moving);
            }

            if (CanCollect(targetCell))
            {
                targetCell.matrixObject.CollectibleObject.Collect(moveInput);
                movement.ExecuteMove(moveInput, GridMovement.MoveState.Moving);
            }

            // GridMovement와 별개로 확실하게 플레이어 상태로 조율
            if (movement.State == GridMovement.MoveState.Moving)
            {
                state = PlayerState.Moving;
            }
        }
    }

    private bool IsDestinationEmpty(MatrixCell targetCell)
    {
        if (targetCell.state == MatrixCell.CellState.Empty) return true;
        
        return false;
    }

    bool CanCollect(MatrixCell targetCell)
    {
        if (targetCell.state == MatrixCell.CellState.Filled)
        {
            if (targetCell.matrixObject.CollectibleObject != null) return true;
        }

        return false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // 1. 현재 들어온 Vector2 값을 읽어옵니다.
        Vector2 rawInput = context.ReadValue<Vector2>();

        // 2. 키를 새로 누르거나 뗐을 때(Performed) 타이밍을 체크합니다.
        if (context.performed)
        {
            // X축(좌우) 입력에 변화가 생겼다면 그 순간의 시간을 기록
            if (rawInput.x != 0 && moveInput.x == 0)
            {
                lastXInputTime = context.time;
            }
            // Y축(상하) 입력에 변화가 생겼다면 그 순간의 시간을 기록
            if (rawInput.y != 0 && moveInput.y == 0)
            {
                lastYInputTime = context.time;
            }
        }

        // 3. 대각선 입력(둘 다 누른 상태)일 때 '더 최근에 누른 축'을 판정합니다.
        if (rawInput.x != 0 && rawInput.y != 0)
        {
            if (lastXInputTime > lastYInputTime)
            {
                // X축이 더 최근이므로 좌우 이동만 남김
                moveInput = new Vector2Int(rawInput.x > 0 ? 1 : -1, 0);
            }
            else
            {
                // Y축이 더 최근이므로 상하 이동만 남김
                moveInput = new Vector2Int(0, rawInput.y > 0 ? 1 : -1);
            }
        }
        else
        {
            // 대각선이 아닐 때는(한쪽 축만 누르거나 다 뗐을 때) 안전하게 반올림 처리
            moveInput = Vector2Int.RoundToInt(rawInput);
        }
    }

    private void OnDestroy()
    {
        if (GamePlayGridManager.Instance.player == this)
        {
            GamePlayGridManager.Instance.player = null;
        }
    }

    public void SetReady()
    {
        state = PlayerState.Ready;
    }

    
}
