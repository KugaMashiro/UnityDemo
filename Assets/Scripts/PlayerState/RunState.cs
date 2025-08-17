using System;
using UnityEngine;

public class RunState : IPlayerState
{
    private readonly PlayerStateManager _stateManger;
    private readonly Action<MovementInputEventArgs> _onMovementInput;
    private readonly Action _onRunButtonPressed;
    private readonly Action _onRollButtonPressed;

    private Vector2 _cachedMovement;
    public RunState(PlayerStateManager manager)
    {
        _stateManger = manager;
        _onMovementInput = OnMovementInput;
        _onRunButtonPressed = OnRunButtunPressed;
        _onRollButtonPressed = OnRollButtonPressed;
    }


    public void Enter()
    {
        EventCenter.OnMovementInput += _onMovementInput;
        EventCenter.OnRunButtunPressed += _onRunButtonPressed;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;

        _cachedMovement = _stateManger.movementInput;
        float clampInput = 0.9f;//Mathf.Clamp(_cachedMovement.magnitude, 0.7f, 0.9f);
        _stateManger.AnimSmoothTransition(AnimParams.MoveState, clampInput, 0.1f);
    }

    public void Update()
    {

    }

    public void Exit()
    {
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnRunButtunPressed -= _onRunButtonPressed;
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;
    }
    public void FixedUpdate()
    {
        HandleMovement();
    }

    private void OnMovementInput(MovementInputEventArgs e)
    {
        _cachedMovement = e.Movement;
        if (!e.HasMovement)
        {
            EventCenter.PublishStateChange(PlayerStateType.Idle);
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDir = _stateManger.GetCameraRelMoveDir(_cachedMovement, Camera.main.transform);
        //Debug.Log(string.Format("in walk state, moveDir = {0}", moveDir));
        // if (!MoveDirUtils.IsValidMoveDirection(moveDir))
        //     return;
        // if (moveDir.sqrMagnitude < 0.01f)
        //         return;

        _stateManger.Controller.ForceFace(moveDir);

        _stateManger.Controller.Move(moveDir, _stateManger.Status.RunSpeed, Time.fixedDeltaTime);
    }

    private void OnRunButtunPressed()
    {
        EventCenter.PublishStateChange(PlayerStateType.Walk);
    }

    private void OnRollButtonPressed()
    {
        EventCenter.PublishStateChange(PlayerStateType.Roll);
    }
}