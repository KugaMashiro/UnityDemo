using System;
using UnityEngine;

public class WalkState: IPlayerState 
{
    private readonly PlayerStateManager _stateManger;
    //private Vector2 movementInput;

    private readonly Action<MovementInputEventArgs> _onMovementInput;
    private readonly Action _onRunButtonPressed;
    private readonly Action<BufferedInputEventArgs> _onRollButtonPressed;

    private Vector2 _cachedMovement;
    //private bool _hasCachedMovement;

    public WalkState(PlayerStateManager manager)
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

        _cachedMovement = _stateManger.MovementInput;

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
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;
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

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        //Debug.Log($"walkstate, {e}, {e.InputUniqueId}");
        //Debug.Log($"{_stateManger.InputBuffer}");
        //_stateManger.InputBuffer.ConsumInputItem(e.InputUniqueId);
        InputBufferSystem.Instance.ConsumInputItem(e.InputUniqueId);
        EventCenter.PublishStateChange(PlayerStateType.Roll);
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