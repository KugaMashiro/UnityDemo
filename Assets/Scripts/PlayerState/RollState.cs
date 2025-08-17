
using System;
using UnityEngine;

public class RollState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;
    private readonly Action _onRollPressed;
    private readonly Action _onAnimRollEnd;

    private Vector3 _initialDir;
    private bool _isBackJump;

    private float _rootTZPercentage;

    private float _movespeed => _isBackJump ? 1f : 2f;

    public RollState(PlayerStateManager manager)
    {
        _stateManager = manager;
        _onAnimRollEnd = OnAnimRollEnd;
    }

    public void Enter()
    {
        EventCenter.OnAnimRollEnd += _onAnimRollEnd;

        _initialDir = _stateManager.GetCameraRelMoveDir(_stateManager.movementInput, Camera.main.transform);
        if (MoveDirUtils.IsValidMoveDirection(_initialDir))
        {
            _isBackJump = false;
            _stateManager.Controller.ForceFace(_initialDir);
        }
        else
        {
            _isBackJump = true;
            _initialDir = _stateManager.Controller.GetCurrentFacing();
        }

        _rootTZPercentage = 0;
        _stateManager.AnimController.SetBool(AnimParams.IsJumpBack, _isBackJump);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Roll);
    }

    public void Exit()
    {
        EventCenter.OnAnimRollEnd -= _onAnimRollEnd;
    }

    public void FixedUpdate()
    {
        //Debug.Log(_stateManager.AnimController.Animator.IsInTransition(0));
        if (!_stateManager.AnimController.IsInTransition(0))
        {
            HandleMovement();
        }
    }

    public void Update()
    {

    }

    private void HandleMovement()
    {
        float curZPercentage = _stateManager.AnimController.GetFloat(AnimParams.RootZTransition);

        //Debug.Log(curZPercentage);

        _stateManager.Controller.Move(_initialDir, (curZPercentage - _rootTZPercentage) * _stateManager.Status.RollDistance);
        _rootTZPercentage = curZPercentage;
        //Debug.Log
    }

    private void OnAnimRollEnd()
    {
        EventCenter.PublishStateChange(PlayerStateType.Idle);
    }
}