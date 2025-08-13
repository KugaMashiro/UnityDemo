using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class PlayerStateManager : MonoBehaviour
{
    [Header("CurrentState")]
    [SerializeField] private string currentStateName;


    [Header("Component Ref")]
    [SerializeField] private CharacterController _controller;
    public CharacterController controller => _controller;
    [SerializeField] private PlayerStatus _status;
    public PlayerStatus status => _status;
    [SerializeField] private Animator _animator;
    public Animator animator => _animator;
    [SerializeField] private Camera mainCamera;

    //[field: SerializeField]
    //[SerializeField]
    private IPlayerState _currentState;
    public IPlayerState currentState => _currentState;

    private readonly Dictionary<PlayerStateType, IPlayerState> _stateMap = new Dictionary<PlayerStateType, IPlayerState>();

    // [HideInInspector] public IdleState idleState { get; private set; }
    // [HideInInspector] public WalkState walkState { get; private set; }
    // [HideInInspector] public RunState runState { get; private set; }

    [HideInInspector] public int animatorMoveState { get; private set; }

    public Vector2 movementInput { get; private set; }

    private Coroutine _currentAnimCoroutine;
    private Action<MovementInputEventArgs> _onMovementInput;
    private Action<StateChangeEventArgs> _onStateChanged;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _status = GetComponent<PlayerStatus>();
        _animator = GetComponentInChildren<Animator>();

        // idleState = new IdleState(this);
        // walkState = new WalkState(this);
        // runState = new RunState(this);
        _stateMap[PlayerStateType.Idle] = new IdleState(this);
        _stateMap[PlayerStateType.Walk] = new WalkState(this);

        if (mainCamera == null)
            mainCamera = Camera.main;

        animatorMoveState = Animator.StringToHash("MoveState");

        _onStateChanged = OnStateChanged;
        _onMovementInput = OnMovementInput;
    }

    private void OnEnable()
    {
        EventCenter.OnStateChange += _onStateChanged;
        EventCenter.OnMovementInput += _onMovementInput;
    }

    private void OnDisable()
    {
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnStateChange -= _onStateChanged;
    }

    private void OnMovementInput(MovementInputEventArgs e)
    {
        movementInput = e.Movement;
    }

    private void OnStateChanged(StateChangeEventArgs e)
    {
        SwitchState(e.TargetState);
    }

    private void Start()
    {
        SwitchState(PlayerStateType.Idle);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (currentState != null)
            currentStateName = currentState.GetType().Name;
        else
            currentStateName = "None";
#endif

        _currentState?.Update();
    }

    private void FixedUpdate()
    {
        _currentState?.FixedUpdate();
    }

    public void SwitchState(PlayerStateType targetStateType)
    {
        if (!_stateMap.TryGetValue(targetStateType, out IPlayerState targetState))
        {
            Debug.LogError($"state type unregirstered: {targetStateType}");
            return;
        }


        if (_currentAnimCoroutine != null)
        {
            StopCoroutine(_currentAnimCoroutine);
            _currentAnimCoroutine = null;
        }

        _currentState?.Exit();
        _currentState = targetState;
        _currentState.Enter();
    }

    public Vector3 GetCameraRelativeMoveDirection(in Vector2 moveInput, in Transform cameraTransform)
    {
        if (moveInput.sqrMagnitude < 0.01f)
            return Vector3.zero;

        Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;

        Vector3 moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;

        return moveDirection;
    }

    public void FaceMoveDirection(in Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, status.faceRotateSpeed * Time.deltaTime);
        }
    }

    public void SetMoveInput(in Vector2 originInput)
    {
        movementInput = originInput;
    }

    public void ResetMoveInput()
    {
        movementInput = Vector2.zero;
    }

    public void StartSmoothAnimTransition(int paramHash, float targetValue, float dampTime)
    {
        if (_currentAnimCoroutine != null)
        {
            StopCoroutine(_currentAnimCoroutine);
        }
        _currentAnimCoroutine = StartCoroutine(SmoothTransitionCoroutine(paramHash, targetValue, dampTime));
    }

    private IEnumerator SmoothTransitionCoroutine(int paramHash, float targetValue, float dampTime)
    {
        const float threshold = 0.01f;

        while (true)
        {
            animator.SetFloat(paramHash, targetValue, dampTime, Time.deltaTime);

            float currentValue = animator.GetFloat(paramHash);
            if (Mathf.Abs(currentValue - targetValue) < threshold)
            {
                animator.SetFloat(paramHash, targetValue);
                break;
            }

            yield return null;
        }

        _currentAnimCoroutine = null;
    }
}
