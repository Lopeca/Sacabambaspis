using System;
using System.Collections;
using System.Collections.Generic;
using _1.Core;
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
    private bool escBuffer;
    private bool spaceBuffer;
    private float spacePressedTime;

    [SerializeField]private PlayerState state;
    public PlayerState State => state;

    private MatrixObject mo;
    public MatrixObject MO => mo;
    GridMovement movement;
    public GridMovement Movement => movement;

    [SerializeField] private GameObject mushroomPrefab; 
    private int mushroomCount;

    private Coroutine controlCoroutine;
    public Action OnDeath;
    private bool isAlive;
    private void Awake()
    {
        state = PlayerState.Uncontrolled;
        mo = GetComponent<MatrixObject>();
        movement = GetComponent<GridMovement>();

        mo.OnEliminated += Die;
        escBuffer = false;
        spaceBuffer = false;

        movement.AfterOnMoveCompleted += PlayerUpdate;
    }

    public void PlayerUpdate()
    {
        if (isAlive && state == PlayerState.Controlled)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (escBuffer)
        {
            PlayerExplode();
            return;
        }
        
        // 어떤 이유로든 이동 중이면 조작을 일단 막음
        if (movement.State != GridMovement.MoveState.Staying)
        {
            state = PlayerState.Uncontrolled;
            controlCoroutine = StartCoroutine(WaitMovement());
            return;
        }
        if (mushroomCount > 0 && spaceBuffer && Time.time - spacePressedTime >= GameConstants.MUSHROOM_SPIT_TIME)
        {
            UseMushroom();
            spaceBuffer = false;
        }
        else if (spaceBuffer && moveInput != Vector2.zero)
        {
            spaceBuffer = false;
            // 제자리에서 옆칸 먹기
        }
        else if (moveInput != Vector2.zero)
        {
            spaceBuffer = false;
       
            
            // 나중에 무턱대고 요청이 아니라 움직일 수 있는지 여기서 확인하고 움직이는 식으로 바꾸기
            // 다른 오브젝트들은 조건을 보고 틀리면 다른 선택을 해야해서 이 요청 함수 안에 들어가서 이동 가능한지 검사하고 이동까지 다 하면 모듈화가 꼬임
             MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.posX + moveInput.x, mo.posY + moveInput.y);

             if (targetCell.state == MatrixCell.CellState.Attacking || targetCell.state == MatrixCell.CellState.Falling)
             {
                 // targetCell.matrixObject.EliminateMatrixObject();
                 // MoveToTargetCell(targetCell);
                 mo.ExplodeOnDeath.Explode();
             }
            else if (IsDestinationEmpty(targetCell))
            {
                movement.ExecuteMove(moveInput, GridMovement.MoveState.Moving, MatrixCell.CellState.Receiving);
            }
            else if (CanCollect(targetCell))
            {
                targetCell.matrixObject.CollectibleObject.Collect(moveInput);
                if(isAlive) // collectible이 trap인경우 이동명령 불가
                    movement.ExecuteMove(moveInput, GridMovement.MoveState.Moving, MatrixCell.CellState.Receiving);
            }
            else if (CanInteract(targetCell))
            {
                targetCell.matrixObject.GridInteractable.Interact(this, moveInput);
            }
        }
    }

    public void PlayerExplode()
    {
        mo.ExplodeOnDeath.Explode();
        
        escBuffer = false;
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

    public void OnEscPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            escBuffer = true;
        }

        if (context.canceled)
        {
            escBuffer = false;
        }
    }

    public void OnSpacePressed(InputAction.CallbackContext context)
    {
        if (state == PlayerState.Uncontrolled) return;
        
        if (context.performed)
        {
            spaceBuffer = true;
            spacePressedTime = Time.time;
        }

        if (context.canceled)
        {
            spaceBuffer = false;
        }
    }
    
    void Die()
    {
        Debug.Log("플레이어 죽음");
        isAlive = false;
        OnDeath?.Invoke();
    }
    
    public void SetReady()
    {
        state = PlayerState.Controlled;
        isAlive = true;
    }

    public void Paralyze()
    {
        state = PlayerState.Uncontrolled;
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
        movement.AfterOnMoveCompleted -= PlayerUpdate;
    }

    public void ObtainMushroom()
    {
        mushroomCount++;
    }

    public void UseMushroom()
    {
        mushroomCount--;
        
        Mushroom mushroom = Instantiate(mushroomPrefab.GetComponent<Mushroom>(), mo.GetCurrentCell().transform, true);
        mushroom.transform.position = transform.position;
        
        mushroom.MO.posX = mo.posX;
        mushroom.MO.posY = mo.posY;
    }
}
