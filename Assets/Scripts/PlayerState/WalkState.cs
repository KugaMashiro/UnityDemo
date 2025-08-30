using System;
using UnityEngine;

public class WalkState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;
    //private Vector2 movementInput;

    private readonly Action<MovementInputEventArgs> _onMovementInput;
    private readonly Action _onRunButtonPressed;
    private readonly Action<BufferedInputEventArgs> _onRollButtonPressed;
    private readonly Action<BufferedInputEventArgs> _onAtkMainPerformed;
    private readonly Action<BufferedInputEventArgs> _onStrongAtkMainPerformed;
    private readonly Action<BufferedInputEventArgs> _onUseItemPressed;
    private readonly Action _onLock;
    private readonly Action _onUnlock;

    private Vector2 _cachedMovement;
    private Vector3 _cachedMoveDir;
    //private bool _hasCachedMovement;

    public WalkState(PlayerStateManager manager)
    {
        _stateManager = manager;
        _onMovementInput = OnMovementInput;
        _onRunButtonPressed = OnRunButtunPressed;
        _onRollButtonPressed = OnRollButtonPressed;
        _onAtkMainPerformed = OnAtkmainPerformed;
        _onStrongAtkMainPerformed = OnStrongAtkmainPerformed;
        _onUseItemPressed = OnUseItemPressed;
        _onLock = OnLock;
        _onUnlock = OnUnLock;
    }

    public void Enter()
    {
        EventCenter.OnMovementInput += _onMovementInput;
        EventCenter.OnRunButtunPressed += _onRunButtonPressed;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed += _onAtkMainPerformed;
        EventCenter.OnStrongAttackMainPerformed += _onStrongAtkMainPerformed;
        EventCenter.OnUseItemPressed += _onUseItemPressed;

        EventCenter.OnLockOnSucceed += _onLock;
        EventCenter.OnLockOnCanceled += _onUnlock;

        _cachedMovement = _stateManager.MovementInput;
        //_cachedMoveDir = _stateManager.GetTargetRelMoveDir(_cachedMovement);
        if (_stateManager.IsLocked)
        {
            _stateManager.AnimController.SetFloat(AnimParams.LockRelativeX, _cachedMovement.x);
            _stateManager.AnimController.SetFloat(AnimParams.LockRelativeZ, _cachedMovement.y);
        }
        else
        {
            _stateManager.AnimController.SetFloat(AnimParams.LockRelativeX, 0f);
            _stateManager.AnimController.SetFloat(AnimParams.LockRelativeZ, 1f);
        }

        float clampInput = 0.55f;//Mathf.Clamp(_cachedMovement.magnitude, 0f, 0.55f);
        _stateManager.AnimSmoothTransition(AnimParams.MoveState, clampInput, 0.1f);
        _stateManager.AnimController.SetAnimStateIndex(AnimStateIndex.Locomotion);
        _stateManager.AnimController.SetMotionType(PlayerMotionType.Walk);
    }

    public void Exit()
    {
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnRunButtunPressed -= _onRunButtonPressed;
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed -= _onAtkMainPerformed;
        EventCenter.OnStrongAttackMainPerformed -= _onStrongAtkMainPerformed;
        EventCenter.OnUseItemPressed -= _onUseItemPressed;

        EventCenter.OnLockOnSucceed -= _onLock;
        EventCenter.OnLockOnCanceled -= _onUnlock;

        //_stateManger.AnimController.SetMotionState(PlayerMotionType.Idle);
    }

    private void OnLock()
    {
        //Debug.Log("lock");
        _stateManager.AnimSmoothTransition(AnimParams.LockRelativeX, _cachedMovement.x,
                AnimParams.LockRelativeZ, _cachedMovement.y, 0.1f);
    }

    private void OnUnLock()
    {
        //Debug.Log("unlock");
        _stateManager.AnimSmoothTransition(AnimParams.LockRelativeX, 0f,
                AnimParams.LockRelativeZ, 1f, 0.1f);
    }

    private void OnMovementInput(MovementInputEventArgs e)
    {
        //Debug.Log($"in walkstate OnMovementInput, MovementInputEventArgs is {e.Movement}, {e.HasMovement}");
        _cachedMovement = e.Movement;
        //_cachedMoveDir = _stateManager.GetTargetRelMoveDir(_cachedMovement);

        //Debug.Log($"{_cachedMovement},  {_cachedMoveDir}");
        //_hasCachedMovement = e.HasMovement;
        //Debug.Log($"in walkstate OnMovementInput, cachedMovement is {_cachedMovement}, hasCachedMovement is {_hasCachedMovement}");
        if (!e.HasMovement)
        {
            EventCenter.PublishStateChange(PlayerStateType.Idle);
            return;
        }

        if (_stateManager.IsLocked)
        {
            _stateManager.AnimSmoothTransition(AnimParams.LockRelativeX, _cachedMovement.x,
                AnimParams.LockRelativeZ, _cachedMovement.y, 0.1f);
            //_stateManager.AnimSmoothTransition(AnimParams.LockRelativeZ, _cachedMovement.y, 0.1f);
        }
    }

    private void OnRunButtunPressed()
    {
        EventCenter.PublishStateChange(PlayerStateType.Run);
    }

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        //Debug.Log($"walkstate, {e}, {e.InputUniqueId}");
        //Debug.Log($"{_stateManger.InputBuffer}");
        //_stateManger.InputBuffer.ConsumInputItem(e.InputUniqueId);
        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);
        EventCenter.PublishStateChange(PlayerStateType.Roll);
    }

    private void OnUseItemPressed(BufferedInputEventArgs e)
    {
        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);
        EventCenter.PublishStateChange(PlayerStateType.UseItem);
    }

    private void HandleMovement()
    {

        Vector3 moveDir;
        //bool isLockOn = false;
        if (!_stateManager.IsLocked)
        {
            moveDir = _stateManager.GetCameraRelMoveDir(_cachedMovement, Camera.main.transform);
            _stateManager.Controller.ForceFace(moveDir);
        }
        else
        {
            moveDir = _stateManager.GetTargetRelMoveDir(_cachedMovement);
            //_stateManager.Controller.ForceFaceTarget(_stateManager.LockOnSystem.LockedTarget.transform);
            _stateManager.Controller.ForceFaceTarget(_stateManager.LockTargetTransform);
        }
        //Debug.Log($"{_cachedMovement}, { moveDir}");
        //Debug.Log(string.Format("in walk state, moveDir = {0}", moveDir));
        // if (!MoveDirUtils.IsValidMoveDirection(moveDir))
        //     return;
        // if (moveDir.sqrMagnitude < 0.01f)
        //         return;


        _stateManager.Controller.Move(moveDir, _stateManager.Status.WalkSpeed, Time.fixedDeltaTime);

    }

    public void FixedUpdate()
    {
        //Debug.Log($"in walkstate fixedupdate, cachedMovement is {_cachedMovement}, hasCachedMovement is {_hasCachedMovement}");
        // if (_hasCachedMovement)
        // {
        // if (!_stateManger.AnimController.IsInTransition(0))
        // {

        HandleMovement();

        //}
        //_hasCachedMovement = false;
        //}
    }

    public void Update()
    {
        // if (_stateManger.movementInput.magnitude < 0.1f)
        // {
        //     _stateManger.SwitchState(_stateManger.idleState);
        // }
        //HandleMovement();
    }

    private void OnAtkmainPerformed(BufferedInputEventArgs e)
    {
        _stateManager.CachedAtkType = AttackType.Light;
        //Debug.Log("Atk light in idle");
        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);
        EventCenter.PublishStateChange(PlayerStateType.Attack);
    }

    private void OnStrongAtkmainPerformed(BufferedInputEventArgs e)
    {
        _stateManager.CachedAtkType = AttackType.Heavy;
        //Debug.Log("Atk light in idle");
        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);
        EventCenter.PublishStateChange(PlayerStateType.Attack);
    }
    
    public void LateUpdate()
    {
        
    }

}