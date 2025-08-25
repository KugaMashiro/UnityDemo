
using System;
using System.Collections.Generic;
using UnityEngine;

public class RollState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;
    private readonly Action<BufferedInputEventArgs> _onRollButtonPressed;

    private readonly Action<MovementInputEventArgs> _onMovementInput;
    private readonly Action _onAnimRollEnd;
    private readonly Action _onAnimInteractWindowOpen;
    private readonly Action<BufferedInputEventArgs> _onAtkMainPerformed;
    private readonly Action<BufferedInputEventArgs> _onStrongAtkMainPerformed;

    //private bool _hasInitValue;
    private Vector3 _initialDir;

    private bool _isBackJump;
    private bool IsBackJump
    {
        get => _isBackJump;
        set
        {
            _isBackJump = value;
            MoveDis = _isBackJump ? _stateManager.Status.JumpBackDistance : _stateManager.Status.RollDistance;
        }
    }

    private float MoveDis;// => _isBackJump ? _stateManager.Status.JumpBackDistance : _stateManager.Status.RollDistance;
    private float? _rootTZPercentage;
    private const float RootTZEpsilon = 0.2f;

    private Vector3 _startTransform;

    private float _movespeed => _isBackJump ? 1f : 2f;

    private List<BufferedInputType> AllowedBufferedInputs { get; }
        = new List<BufferedInputType> { BufferedInputType.AttackLight, BufferedInputType.Roll, BufferedInputType.AttackHeavy };

    private bool _canInteract;
    private bool _isRollTransitionPending = false;


    public RollState(PlayerStateManager manager)
    {
        //_hasInitValue = false;
        _stateManager = manager;
        _onAnimRollEnd = OnAnimRollEnd;
        _onRollButtonPressed = OnRollButtonPressed;
        _onAnimInteractWindowOpen = OnAnimInteractWindowOpen;
        _onMovementInput = OnMovementInput;
        _onAtkMainPerformed = OnAtkmainPerformed;
        _onStrongAtkMainPerformed = OnStrongAtkmainPerformed;
    }

    public void Enter()
    {
        _startTransform = _stateManager.transform.position;
        EventCenter.OnAnimRollEnd += _onAnimRollEnd;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;
        EventCenter.OnAnimInteractWindowOpen += _onAnimInteractWindowOpen;
        EventCenter.OnMovementInput += _onMovementInput;
        EventCenter.OnAttackMainPerformed += _onAtkMainPerformed;
        EventCenter.OnStrongAttackMainPerformed += _onStrongAtkMainPerformed;


        _stateManager.AnimController.SetAnimStateIndex(AnimStateIndex.RollAndJumpBack);
        _stateManager.AnimController.SetMotionType(PlayerMotionType.Idle);

        StartRolling();

        // GetInitialDir();

        // _rootTZPercentage = null;
        // _canInteract = false;
        // _stateManager.AnimController.SetBool(AnimParams.IsJumpBack, IsBackJump);
        // _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Roll);

        //_stateManager.AnimController.SetBool(Animator.StringToHash("IsRolling"), true);
    }
    public void Exit()
    {
        EventCenter.OnAnimRollEnd -= _onAnimRollEnd;
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;
        EventCenter.OnAnimInteractWindowOpen -= _onAnimInteractWindowOpen;
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnAttackMainPerformed -= _onAtkMainPerformed;
        EventCenter.OnStrongAttackMainPerformed -= _onStrongAtkMainPerformed;

        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_Roll);
        _stateManager.AnimController.SetBool(Animator.StringToHash("IsRolling"), false);

        //Debug.Log($"Rollstate Trans: {(_stateManager.transform.position - _startTransform).magnitude}");
    }

    private void StartRolling()
    {
        GetInitialDir();
        _rootTZPercentage = null;
        _canInteract = false;
        _isRollTransitionPending = true;

        _stateManager.AnimController.SetBool(AnimParams.IsJumpBack, IsBackJump);
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_Roll);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Roll);
    }

    public void GetInitialDir()
    {
        if (!_stateManager.CachedDir.HasValue)
        {
            _initialDir = _stateManager.GetCameraRelMoveDir(_stateManager.MovementInput, Camera.main.transform);
        }
        else
        {
            _initialDir = _stateManager.CachedDir.Value;
            _stateManager.CachedDir = null;
        }
        //Debug.Log($"{ _initialDir}, {MoveDirUtils.IsValidMoveDirection(_initialDir)}");
        if (MoveDirUtils.IsValidMoveDirection(_initialDir))
        {
            IsBackJump = false;
            _stateManager.Controller.ForceFace(_initialDir);
        }
        else
        {
            IsBackJump = true;
            _initialDir = _stateManager.Controller.GetCurrentFacing();
        }
    }

    public void FixedUpdate()
    {
        //Debug.Log(_stateManager.AnimController.Animator.IsInTransition(0));
        // if (!_stateManager.AnimController.IsInTransition(0))
        // {
        //Debug.Log($"{_stateManager.AnimController.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime}");
        //Debug.Log($"runing clips:{_stateManager.AnimController.Animator.runtimeAnimatorController.animationClips.Length}");
        //Debug.Log($"animator state: {_stateManager.AnimController.Animator.GetAnimatorTransitionInfo(0)}");

        bool isInTransition = _stateManager.AnimController.IsInTransition(0);

        if (isInTransition || _isRollTransitionPending)
        {
            if (isInTransition)
            {
                _isRollTransitionPending = false;
                // _rootTZPercentage = _stateManager.AnimController.GetFloat(AnimParams.RootZTransitionL0);
                // Debug.Log(_rootTZPercentage);
                // return;
            }
            return;
        }

        // if (!_stateManager.AnimController.IsInTransition(0))
        // {
        HandleMovement();
        //}

        //TryFixedMove();

        //}
        //HandleMovement();
    }

    public void Update()
    {

    }

    private void HandleMovement()
    {

        //Debug.Log(curZPercentage);

        // if (!_stateManager.AnimController.IsInTransition(0))
        // {
        float curZPercentage = _stateManager.AnimController.GetFloat(AnimParams.RootZTransitionL0);
        // if (curZPercentage - _rootTZPercentage < 0)
        // {
        //     Debug.Log(curZPercentage - _rootTZPercentage);
        //     Debug.Log($"{_stateManager.AnimController.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime}");
        // }
        if (_rootTZPercentage.HasValue)
        {
            // if (curZPercentage - _rootTZPercentage.Value < -0.1)
            //     Debug.Log($"encountered! {curZPercentage - _rootTZPercentage.Value}, {_stateManager.AnimController.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash}");
            if (Mathf.Abs(curZPercentage - _rootTZPercentage.Value) < GlobalConstants.ROOTTZ_EPLSON)
            _stateManager.Controller.Move(_initialDir,
                (curZPercentage - _rootTZPercentage.Value) * MoveDis);
        }
        // else
        // {
        //     if(Mathf.Abs(curZPercentage) < RootTZEpsilon)
        //         _rootTZPercentage = curZPercentage;
        // }
        // }
        _rootTZPercentage = curZPercentage;
        //Debug.Log
    }

    private void TryFixedMove()
    {
        _stateManager.Controller.Move(_initialDir, _movespeed * Time.fixedDeltaTime);
    }

    private void OnAnimRollEnd()
    {
        // else
        // {
        //_stateManager.CachedDir = null;
        if (MoveDirUtils.IsValidMoveDirection(_stateManager.MovementInput))
        {
            EventCenter.PublishStateChange(PlayerStateType.Walk);
            return;
        }
        else
        {
            EventCenter.PublishStateChange(PlayerStateType.Idle);
            return;
        }
        // }
    }

    private void OnAnimInteractWindowOpen()
    {
        _canInteract = true;
        HandleBufferedInput();
        // if (MoveDirUtils.IsValidMoveDirection(_stateManager.MovementInput))
        // {
        //     EventCenter.PublishStateChange(PlayerStateType.Walk);
        //     //return;
        // }
    }

    private void HandleBufferedInput()
    {
        InputBufferItem bufferedInput = _stateManager.GetValidInput(AllowedBufferedInputs);

        if (bufferedInput != null)
        {
            if (bufferedInput.InputType == BufferedInputType.Roll)
            {
                // _stateManager.CachedDir = bufferedInput.BufferedDir;
                // InputBufferSystem.Instance.ConsumeInputItem(bufferedInput.UniqueId);
                _stateManager.CacheDirAndComsumeInputBuffer(bufferedInput);
                Debug.Log("using buffer");
                StartRolling();
                //EventCenter.PublishStateChange(PlayerStateType.Roll);
                return;
            }
            else if (InputToAttackTypeMap.TryGet(bufferedInput.InputType, out var inputAtkType))
            {
                _stateManager.CacheDirAndComsumeInputBuffer(bufferedInput);
                _stateManager.CachedAtkType = inputAtkType;
                _stateManager.CachedInputCanceled = bufferedInput.ReleaseTime.HasValue;

                EventCenter.PublishStateChange(PlayerStateType.Attack);
                return;
            }
        }

        if (MoveDirUtils.IsValidMoveDirection(_stateManager.MovementInput))
        {
            EventCenter.PublishStateChange(PlayerStateType.Walk);
            //return;
        }
    }

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        if (!_canInteract) return;
        _canInteract = false;
        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);

        StartRolling();
    }

    private void OnMovementInput(MovementInputEventArgs e)
    {
        if (!_canInteract) return;
        _canInteract = false;
        if (e.HasMovement)
        {
            EventCenter.PublishStateChange(PlayerStateType.Walk);
        }
    }
    
    private void OnAtkmainPerformed(BufferedInputEventArgs e)
    {
        if (!_canInteract) return;
        _canInteract = false;
        _stateManager.CachedAtkType = AttackType.Light;
        //Debug.Log("Atk light in idle");
        EventCenter.PublishStateChange(PlayerStateType.Attack);
    }

    private void OnStrongAtkmainPerformed(BufferedInputEventArgs e)
    {
        if (!_canInteract) return;
        _canInteract = false;
        _stateManager.CachedAtkType = AttackType.Heavy;
        //Debug.Log("Atk light in idle");
        EventCenter.PublishStateChange(PlayerStateType.Attack);
    }
}