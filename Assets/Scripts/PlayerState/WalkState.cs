using System;
using UnityEngine;

public class WalkState: IPlayerState 
{
    private readonly PlayerStateManager _stateManger;
    //private Vector2 movementInput;

    private readonly Action<MovementInputEventArgs> _onMovementInput;
    private readonly Action _onRunButtonPressed;

    private Vector2 _cachedMovement;
    //private bool _hasCachedMovement;

    public WalkState(PlayerStateManager manager)
    {
        _stateManger = manager;
        _onMovementInput = OnMovementInput;
        _onRunButtonPressed = OnRunButtunPressed;
    }

    public void Enter()
    {
        EventCenter.OnMovementInput += _onMovementInput;
        EventCenter.OnRunButtunPressed += _onRunButtonPressed;

        _cachedMovement = _stateManger.movementInput;

        float clampInput = 0.55f;//Mathf.Clamp(_cachedMovement.magnitude, 0f, 0.55f);
        _stateManger.AnimSmoothTransition(AnimParams.MoveState, clampInput, 0.1f);
    }

    public void Update() 
    {
        // if (_stateManger.movementInput.magnitude < 0.1f)
        // {
        //     _stateManger.SwitchState(_stateManger.idleState);
        // }
    }

    public void Exit()
    {
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnRunButtunPressed -= _onRunButtonPressed;
    }

    private void OnMovementInput(MovementInputEventArgs e)
    {   
        //Debug.Log($"in walkstate OnMovementInput, MovementInputEventArgs is {e.Movement}, {e.HasMovement}");
        _cachedMovement = e.Movement;
        //_hasCachedMovement = e.HasMovement;
        //Debug.Log($"in walkstate OnMovementInput, cachedMovement is {_cachedMovement}, hasCachedMovement is {_hasCachedMovement}");
        if (!e.HasMovement)
        {
            EventCenter.PublishStateChange(PlayerStateType.Idle);
        }
    }

    private void OnRunButtunPressed()
    {
        EventCenter.PublishStateChange(PlayerStateType.Run);
    }

    private void HandleMovement()
    {
        Vector3 moveDir = _stateManger.GetCameraRelMoveDir(_cachedMovement, Camera.main.transform);
        //Debug.Log(string.Format("in walk state, moveDir = {0}", moveDir));
        // if (!MoveDirUtils.IsValidMoveDirection(moveDir))
        //     return;
        // if (moveDir.sqrMagnitude < 0.01f)
        //         return;

        _stateManger.Controller.Face(moveDir);

        _stateManger.Controller.Move(moveDir, _stateManger.Status.WalkSpeed, Time.fixedDeltaTime);

    }

    public void FixedUpdate()
    {
        //Debug.Log($"in walkstate fixedupdate, cachedMovement is {_cachedMovement}, hasCachedMovement is {_hasCachedMovement}");
        // if (_hasCachedMovement)
        // {
        HandleMovement();
            //_hasCachedMovement = false;
        //}
    }
}