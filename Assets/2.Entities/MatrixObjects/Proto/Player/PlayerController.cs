using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Controlled,  // 제어 하에 있는
        Uncontrolled // 제어에서 벗어난
    }
    
    private double lastXInputTime;
    private double lastYInputTime;
    private Vector2Int moveInput;

    [SerializeField]private PlayerState state;
    public PlayerState State => state;

    private MatrixObject mo;
    public MatrixObject MO => mo;
    GridMovement movement;
    public GridMovement Movement => movement;
    Coroutine controlCoroutine;

    public Action OnDeath;
    private void Awake()
    {
        state = PlayerState.Uncontrolled;
        mo = GetComponent<MatrixObject>();
        movement = GetComponent<GridMovement>();

        mo.OnEliminated += Die;
    }

    public void PlayerUpdate()
    {
        if (state == PlayerState.Controlled)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (moveInput != Vector2.zero)
        {
            // 어떤 이유로든 이동 중이면 조작을 일단 막음
            if (movement.State != GridMovement.MoveState.Staying)
            {
                state = PlayerState.Uncontrolled;
                controlCoroutine = StartCoroutine(WaitMovement());
                return;
            }
            // 나중에 무턱대고 요청이 아니라 움직일 수 있는지 여기서 확인하고 움직이는 식으로 바꾸기
            // 다른 오브젝트들은 조건을 보고 틀리면 다른 선택을 해야해서 이 요청 함수 안에 들어가서 이동 가능한지 검사하고 이동까지 다 하면 모듈화가 꼬임
             MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.posX + moveInput.x, mo.posY + moveInput.y);

             if (targetCell.state == MatrixCell.CellState.Attacking)
             {
                 targetCell.matrixObject.EliminateMatrixObject();
                 MoveToTargetCell(targetCell);
                 mo.ExplodeOnDeath.Explode();
             }
            else if (IsDestinationEmpty(targetCell))
            {
                movement.ExecuteMove(moveInput, GridMovement.MoveState.Moving, MatrixCell.CellState.Moving);
            }
            else if (CanCollect(targetCell))
            {
                targetCell.matrixObject.CollectibleObject.Collect(moveInput);
                movement.ExecuteMove(moveInput, GridMovement.MoveState.Moving, MatrixCell.CellState.Moving);
            }
            else if (CanInteract(targetCell))
            {
                targetCell.matrixObject.GridInteractable.Interact(this, moveInput);
            }
        }
    }

    private void MoveToTargetCell(MatrixCell targetCell)
    {
        targetCell.PutMatrixObject(mo);
    }

    private bool CanInteract(MatrixCell targetCell)
    {
        return targetCell.state == MatrixCell.CellState.Filled && targetCell.matrixObject.GridInteractable != null;
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

    public void Suicide()
    {
        mo.ExplodeOnDeath.Explode();
    }
    
    void Die()
    {
        OnDeath?.Invoke();
    }
    
    public void SetReady()
    {
        state = PlayerState.Controlled;
    }

    IEnumerator WaitMovement()
    {
        while (movement.State != GridMovement.MoveState.Staying)
            yield return null;
        
        state = PlayerState.Controlled;
    }

    private void OnDestroy()
    {
        mo.OnEliminated -= Die;
    }
}
