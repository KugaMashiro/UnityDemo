
using System;
using System.Collections.Generic;
using UnityEngine;

public class RollState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;
    private readonly Action _onRollPressed;
    private readonly Action _onAnimRollEnd;

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

    private Vector3 _startTransform;

    private float _movespeed => _isBackJump ? 1f : 2f;

    private List<BufferedInputType> AllowedBufferedInputs { get; }
        = new List<BufferedInputType> { BufferedInputType.Roll };

    public RollState(PlayerStateManager manager)
    {
        //_hasInitValue = false;
        _stateManager = manager;
        _onAnimRollEnd = OnAnimRollEnd;
    }

    public void Enter()
    {
        _startTransform = _stateManager.transform.position;
        EventCenter.OnAnimRollEnd += _onAnimRollEnd;

        if (!_stateManager.CachedDir.HasValue)
        {
            _initialDir = _stateManager.GetCameraRelMoveDir(_stateManager.MovementInput, Camera.main.transform);
        }
        else
        {
            _initialDir = _stateManager.CachedDir.Value;
            _stateManager.CachedDir = null;
        }
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

        _rootTZPercentage = null;
        _stateManager.AnimController.SetBool(AnimParams.IsJumpBack, IsBackJump);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Roll);
    }

    public void Exit()
    {
        EventCenter.OnAnimRollEnd -= _onAnimRollEnd;
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_Roll);

        Debug.Log($"Rollstate Trans: {(_stateManager.transform.position - _startTransform).magnitude}");
    }

    public void FixedUpdate()
    {
        //Debug.Log(_stateManager.AnimController.Animator.IsInTransition(0));
        // if (!_stateManager.AnimController.IsInTransition(0))
        // {
        //Debug.Log($"{_stateManager.AnimController.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime}");
        //Debug.Log($"runing clips:{_stateManager.AnimController.Animator.runtimeAnimatorController.animationClips.Length}");
        //Debug.Log($"animator state: {_stateManager.AnimController.Animator.GetAnimatorTransitionInfo(0)}");

        if (!_stateManager.AnimController.IsInTransition(0))
        {
            HandleMovement();
        }

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
        float curZPercentage = _stateManager.AnimController.GetFloat(AnimParams.RootZTransition);
        // if (curZPercentage - _rootTZPercentage < 0)
        // {
        //     Debug.Log(curZPercentage - _rootTZPercentage);
        //     Debug.Log($"{_stateManager.AnimController.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime}");
        // }
        if (_rootTZPercentage.HasValue)
        {
            _stateManager.Controller.Move(_initialDir,
                (curZPercentage - _rootTZPercentage.Value) * MoveDis);
        }
        _rootTZPercentage = curZPercentage;
        // }
        //Debug.Log
    }

    private void TryFixedMove()
    {
        _stateManager.Controller.Move(_initialDir, _movespeed * Time.fixedDeltaTime);
    }

    private void OnAnimRollEnd()
    {
        InputBufferItem bufferedInput = _stateManager.GetValidInput(AllowedBufferedInputs);

        if (bufferedInput != null)
        {
            if (bufferedInput.InputType == BufferedInputType.Roll)
            {
                _stateManager.CachedDir = bufferedInput.BufferedDir;
                InputBufferSystem.Instance.ConsumInputItem(bufferedInput.UniqueId);
                EventCenter.PublishStateChange(PlayerStateType.Roll);
                return;
            }
        }
        else
        {
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
        }
    }
}