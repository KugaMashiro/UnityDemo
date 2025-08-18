using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class PlayerStateManager : MonoBehaviour
{
    [Header("CurrentState")]
    [SerializeField] private string currentStateName;


    [Header("Component Ref")]
    [SerializeField] private PlayerLocomotion _controller;
    [SerializeField] private PlayerStatus _status;
    [SerializeField] private PlayerAnimController _animController;
    //[SerializeField] private InputBufferSystem _inputbuffer; 
    [SerializeField] private Camera mainCamera;

    public PlayerLocomotion Controller => _controller;
    public PlayerAnimController AnimController => _animController;
    public PlayerStatus Status => _status;

    //public InputBufferSystem InputBuffer;

    private IPlayerState _currentState;
    public IPlayerState currentState => _currentState;

    private readonly Dictionary<PlayerStateType, IPlayerState> _stateMap = new Dictionary<PlayerStateType, IPlayerState>();
    public Vector2 MovementInput { get; private set; }
    public Vector3? CachedDir { get; set; }

    private Action<MovementInputEventArgs> _onMovementInput;
    private Action<StateChangeEventArgs> _onStateChanged;
    private void Awake()
    {
        _controller = GetComponent<PlayerLocomotion>();
        _status = GetComponent<PlayerStatus>();
        _animController = GetComponent<PlayerAnimController>();
        //_inputbuffer = InputBufferSystem.Instance;

        InitStateMap();

        if (mainCamera == null)
            mainCamera = Camera.main;

        _onStateChanged = OnStateChanged;
        _onMovementInput = OnMovementInput;
    }

    private void InitStateMap()
    {
        _stateMap[PlayerStateType.Idle] = new IdleState(this);
        _stateMap[PlayerStateType.Walk] = new WalkState(this);
        _stateMap[PlayerStateType.Run] = new RunState(this);
        _stateMap[PlayerStateType.Roll] = new RollState(this);
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
        MovementInput = e.Movement;
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

        _currentState?.Exit();
        _currentState = targetState;
        _currentState.Enter();
    }

    // public Vector3 GetCameraRelativeMoveDirection(in Vector2 moveInput, in Transform cameraTransform)
    // {
    //     if (moveInput.sqrMagnitude < 0.01f)
    //         return Vector3.zero;

    //     Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
    //     Vector3 cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;

    //     Vector3 moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;

    //     return moveDirection;
    // }

    // public void FaceMoveDirection(in Vector3 moveDir)
    // {
    //     if (moveDir.sqrMagnitude > 0.01f)
    //     {
    //         Quaternion targetRotation = Quaternion.LookRotation(moveDir);
    //         transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Status.faceRotateSpeed * Time.deltaTime);
    //     }
    // }

    public void SetMoveInput(in Vector2 originInput)
    {
        MovementInput = originInput;
    }

    public void ResetMoveInput()
    {
        MovementInput = Vector2.zero;
    }

    public InputBufferItem GetValidInput(List<BufferedInputType> allowedTypes)
    {
        return InputBufferSystem.Instance.GetValidInput(allowedTypes);
    }
}

public static class PlayerStateManagerExtensions
{
    public static void AnimSmoothTransition(this PlayerStateManager manager, int paramHash, float targetValue, float dampTime)
    {
        manager.AnimController.SmoothTransition(paramHash, targetValue, dampTime);
    }

    public static Vector3 GetCameraRelMoveDir(this PlayerStateManager manager, Vector2 moveInput, Transform cameraTransform)
    {
        return manager.Controller.GetCameraRelativeMoveDirection(moveInput, cameraTransform);
    }

    public static Vector3 GetCameraRelMoveDir(this PlayerStateManager manager)
    {
        return manager.Controller.GetCameraRelativeMoveDirection(manager.MovementInput, Camera.main.transform);
    }
}
