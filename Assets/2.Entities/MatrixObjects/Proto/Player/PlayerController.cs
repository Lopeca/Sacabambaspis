using System;
using System.Collections;
using System.Collections.Generic;
using _1.Core;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    private static readonly int IsEww = Animator.StringToHash("IsEww");
    private static readonly int InputX = Animator.StringToHash("InputX");
    private static readonly int InputY = Animator.StringToHash("InputY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsMovingL = Animator.StringToHash("IsMovingL");

    public enum PlayerState
    {
        Controlled,  // 제어 하에 있는
        Uncontrolled // 제어에서 벗어난
    }
    
    private double lastXInputTime;
    private double lastYInputTime;
    [SerializeField] private Vector2Int moveInput;
    private bool escBuffer;
    [SerializeField] private bool spaceBuffer;
    [SerializeField] private bool spaceBufferUsed;
    
    private float spacePressedTime;
    private bool spaceMoveLock;
    private int moveTicks;
    public int MoveTicks => moveTicks;

    [SerializeField]private PlayerState state;
    public PlayerState State => state;

    private MatrixObject mo;
    public MatrixObject MO => mo;
    GridMovement movement;
    public GridMovement Movement => movement;

    [SerializeField] private GameObject mushroomPrefab; 
    private int mushroomCount;
    public int MushroomCount => mushroomCount;

    private Coroutine controlCoroutine;
    public Action OnDeath;
    public Action MushroomUseAction;
    private bool isAlive;
    
    [SerializeField] AudioClip deathSound;
    [SerializeField] public AudioClip pushVoice;
    [SerializeField] private AudioClip suicideVoice;

    private int horizontalDirection;
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

    private void Start()
    {
        moveTicks = Mathf.RoundToInt(movement.physicsSO.moveDuration / Time.fixedDeltaTime);
    }

    public void PlayerUpdate()
    {
        if (spaceBuffer && state == PlayerState.Uncontrolled)
        {
            spacePressedTime = Time.time; //
        }

        // [핵심] 이동이 완료된 상태(Staying)라면 즉시 제어 권한을 복구합니다.
        if (movement.State == GridMovement.MoveState.Staying)
        {
            state = PlayerState.Controlled;
        }

        if (isAlive && state == PlayerState.Controlled)
        {
            HandleInput(); //[cite: 6]
        }
    }

    private void HandleInput()
    {
        if (escBuffer)
        {
            SoundManager.Instance.PlayGlobalGameSFX(suicideVoice, 1f, 1, 1, 1);
            PlayerExplode();
            return;
        }

        if (!spaceBuffer)
        {
            MO.Animator.SetBool(IsEww, false);
            MO.Animator.SetFloat(InputX, 0);
            MO.Animator.SetFloat(InputY, 0);
            spaceBufferUsed = false;
        }
        
        // 어떤 이유로든 이동 중이면 조작을 일단 막음
        if (movement.State != GridMovement.MoveState.Staying)
        {
            state = PlayerState.Uncontrolled;
            controlCoroutine = StartCoroutine(WaitMovement());
            return;
        }
        
        MO.Animator.SetBool(IsMoving, false);
        MO.Animator.SetBool(IsMovingL, false);
        MO.SpriteRenderer.flipX = false;

        if (moveInput.x > 0) horizontalDirection = 1;
        else if (moveInput.x < 0) horizontalDirection = -1;

        float spaceChargedTime = Time.time - spacePressedTime;

        if (mushroomCount > 0 && spaceBuffer  && !spaceBufferUsed && spaceChargedTime is >= GameConstants.MUSHROOM_EWW_TIME and < GameConstants.MUSHROOM_SPIT_TIME)
        {
            MO.Animator.SetBool(IsEww, true);
            MO.Animator.Play("Eww");
        }
        else if (mushroomCount > 0 && spaceBuffer && !spaceBufferUsed && spaceChargedTime >= GameConstants.MUSHROOM_SPIT_TIME)
        {
            UseMushroom();
            MO.Animator.SetBool(IsEww, false);
            
            spaceBuffer = false;
        }
        else if (spaceBuffer && moveInput != Vector2.zero)
        {
            spaceMoveLock = true;
            spaceBufferUsed = true;
            MO.Animator.SetBool(IsEww, false);
            
            MO.Animator.SetFloat(InputX, moveInput.x);
            MO.Animator.SetFloat(InputY, moveInput.y);
            // 제자리에서 옆칸 먹기
            MatrixCell targetCell = GamePlayGridManager.Instance.GetCell(mo.posX + moveInput.x, mo.posY + moveInput.y);
            if (CanCollect(targetCell))
            {
                targetCell.matrixObject.CollectibleObject.Collect(Vector2Int.zero);
            }
            else if (CanInteract(targetCell))
            {
                targetCell.matrixObject.GridInteractable.Interact(this, Vector2Int.zero);
            }

        }
        else if (moveInput != Vector2.zero && !spaceMoveLock)
        {
            spaceBuffer = false;
            spaceBufferUsed = true;
            MO.Animator.SetFloat(InputX, 0);
            MO.Animator.SetFloat(InputY, 0);
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
                if (horizontalDirection == -1)
                {
                    MO.Animator.SetBool(IsMovingL, true);
                }
                else
                {
                    MO.Animator.SetBool(IsMoving, true);
                }
            }
            else if (CanCollect(targetCell))
            {
                if (horizontalDirection == -1)
                {
                    MO.Animator.SetBool(IsMovingL, true);
                }
                else
                {
                    MO.Animator.SetBool(IsMoving, true);
                }
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
            if (targetCell.matrixObject.CollectibleObject != null &&
                targetCell.matrixObject.CollectibleObject.collected == false)
            {
                return true;
            }
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
        if (context.performed)
        {
            spaceBuffer = true;
            spacePressedTime = Time.time;
        }

        if (context.canceled)
        {
            spaceBuffer = false;
            spaceMoveLock = false;
        }
    }
    
    void Die()
    {
        Debug.Log("플레이어 죽음");
        if(isAlive) SoundManager.Instance.PlayGameSFX(deathSound, transform.position);
        isAlive = false;
        if(!GamePlayGridManager.Instance.isCleared) OnDeath?.Invoke();
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
        
        MushroomUseAction?.Invoke();
    }
}
