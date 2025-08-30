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

public enum PlayerStateType
{
    Idle,
    Walk,
    Run,
    Roll,
    Attack,
    Hit,
    UseItem
}

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

    [SerializeField] private LockOnSystem _lockOnSystem;
    [SerializeField] private InventoryManager _inventoryManager;
    public bool IsLocked => _lockOnSystem.IsLocked;
    public Transform LockTargetTransform => _lockOnSystem.LockedTarget.transform;

    [Header("Base Animator Layer")]
    [SerializeField] private AnimatorController _baseController;
    private AnimatorController _combinedController;

    public PlayerLocomotion Controller => _controller;
    public PlayerAnimController AnimController => _animController;
    public PlayerStatus Status => _status;
    public LockOnSystem LockOnSystem => _lockOnSystem;
    public InventoryManager Inventory => _inventoryManager;


    //public InputBufferSystem InputBuffer;

    private IPlayerState _currentState;
    public IPlayerState currentState => _currentState;

    private readonly Dictionary<PlayerStateType, IPlayerState> _stateMap = new Dictionary<PlayerStateType, IPlayerState>();
    public Vector2 MovementInput { get; private set; }
    public Vector3? CachedDir { get; set; }
    public AttackType CachedAtkType { get; set; }
    public bool CachedInputCanceled { get; set; }
    private Action<MovementInputEventArgs> _onMovementInput;
    private Action<StateChangeEventArgs> _onStateChanged;

    [SerializeField] private List<WeaponData> _weaponDatas;
    private int _currentWeaponIndex = 0;
    public Dictionary<int, int> WeaponAnimLayerMapping = new Dictionary<int, int>();
    public WeaponData CurrentWeapon => _weaponDatas.Count > 0
        ? _weaponDatas[_currentWeaponIndex]
        : null;
    private void Awake()
    {
        _controller = GetComponent<PlayerLocomotion>();
        _status = GetComponent<PlayerStatus>();
        _animController = GetComponent<PlayerAnimController>();
        _lockOnSystem = GetComponent<LockOnSystem>();
        _inventoryManager = GetComponent<InventoryManager>();
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
        _stateMap[PlayerStateType.Attack] = new AttackState(this);
        _stateMap[PlayerStateType.Hit] = new HitState(this);
        _stateMap[PlayerStateType.UseItem] = new UseItemState(this);
    }

    private void OnEnable()
    {
        EventCenter.OnStateChange += _onStateChanged;
        EventCenter.OnMovementInput += _onMovementInput;

        EventCenter.OnHit += OnHit;
    }

    private void OnDisable()
    {
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnStateChange -= _onStateChanged;

        EventCenter.OnHit -= OnHit;
    }

    private void OnMovementInput(MovementInputEventArgs e)
    {
        MovementInput = e.Movement;
        // if (LockOnSystem.IsLocked)
        // {
        //     _animController.Animator.SetFloat()
        // }
    }

    private void OnStateChanged(StateChangeEventArgs e)
    {
        SwitchState(e.TargetState);
    }

    private void OnHit()
    {
        SwitchState(PlayerStateType.Hit);
    }

    private void Start()
    {
        //_animController.Animator.runtimeAnimatorController = _baseController;
        CombineController();
        _animController.Animator.runtimeAnimatorController = _combinedController;
        _animController.Animator.SetLayerWeight(WeaponAnimLayerMapping[_currentWeaponIndex], 1f);
        SwitchState(PlayerStateType.Idle);
    }

    public int GetCurWeaponAnimLayerIndex()
    {
        return WeaponAnimLayerMapping[_currentWeaponIndex];
    }

    private void CombineController()
    {
        WeaponAnimLayerMapping.Clear();
        _combinedController = Instantiate(_baseController);
        List<AnimatorControllerLayer> layers = new List<AnimatorControllerLayer>();

        if (_combinedController != null && _combinedController.layers.Length > 0)
        {
            AnimatorControllerLayer baseLayer = _combinedController.layers[0];
            baseLayer.name = "BaseLayer";
            baseLayer.defaultWeight = 1f;
            layers.Add(baseLayer);

            AnimatorControllerLayer ItemLayer = _combinedController.layers[1];
            ItemLayer.name = "ItemLayer";
            ItemLayer.blendingMode = AnimatorLayerBlendingMode.Override;
            ItemLayer.defaultWeight = 0f;
            layers.Add(ItemLayer);
        }

        int baseLayerNum = layers.Count;

        for (int i = 0; i < _weaponDatas.Count; i++)
        {
            WeaponData weapon = _weaponDatas[i];
            if (weapon == null || weapon.attackLayerController == null) continue;

            AnimatorControllerLayer weaponLayer = weapon.GetAnimatorControllerLayer();
            weaponLayer.name = $"WeaponLayer_{weapon.weaponName}";
            weaponLayer.blendingMode = AnimatorLayerBlendingMode.Override;
            weaponLayer.defaultWeight = 0f;

            layers.Add(weaponLayer);
            WeaponAnimLayerMapping[i] = i + baseLayerNum;
        }

        _combinedController.layers = layers.ToArray();
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

    private void LateUpdate()
    {
        //Debug.Log("LateUpdate");
        _currentState?.LateUpdate();
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

    public AnimatorStateInfo AnimBaseLayerInfo()
    {
        return this.AnimController.Animator.GetCurrentAnimatorStateInfo((int)AnimLayer.Base);
    }

    public AnimatorStateInfo AnimItemLayerInfo()
    {
        return this.AnimController.Animator.GetCurrentAnimatorStateInfo((int)AnimLayer.Item);
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

    public ItemData GetCurItemData()
    {
        return _inventoryManager.CurrentItem.itemData;
    }
}

public static class PlayerStateManagerExtensions
{
    public static void AnimSmoothTransition(this PlayerStateManager manager, int paramHash, float targetValue, float dampTime)
    {
        manager.AnimController.SmoothTransition(paramHash, targetValue, dampTime);
    }

    public static void AnimSmoothTransition(this PlayerStateManager manager, int param1, float targetValue1,
        int param2, float targetValue2, float dampTime)
    {
        manager.AnimController.SmoothTransition(param1, targetValue1, param2, targetValue2, dampTime);
    }


    public static Vector3 GetCameraRelMoveDir(this PlayerStateManager manager, Vector2 moveInput, Transform cameraTransform)
    {
        return manager.Controller.GetCameraRelativeMoveDirection(moveInput, cameraTransform);
    }

    public static Vector3 GetTargetRelMoveDir(this PlayerStateManager manager, Vector2 moveInput)
    {
        return manager.Controller.GetTargetRelativeMoveDirection(moveInput, manager.LockOnSystem.LockedTarget.transform);
    }

    public static Vector2 GetInputFromMoveDirection(this PlayerStateManager manager, Vector3 dir)
    {
        return manager.Controller.GetInputFromMoveDirection(dir, manager.transform, manager.LockOnSystem.LockedTarget.transform);
    }

    public static Vector3 GetCameraRelMoveDir(this PlayerStateManager manager)
    {
        return manager.Controller.GetCameraRelativeMoveDirection(manager.MovementInput, Camera.main.transform);
    }

    public static Vector3 GetRelMoveDir(this PlayerStateManager manager)
    {
        if (manager.IsLocked)
        {
            return manager.GetTargetRelMoveDir(manager.MovementInput);
        }
        else
        {
            return manager.GetCameraRelMoveDir();
        }
    }

    public static void CacheDirAndComsumeInputBuffer(this PlayerStateManager manager, InputBufferItem item)
    {
        manager.CachedDir = item.BufferedDir;
        InputBufferSystem.Instance.ConsumeInputItem(item.UniqueId);
    }
}
