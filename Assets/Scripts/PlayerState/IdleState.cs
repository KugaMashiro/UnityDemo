using System;
using Unity.VisualScripting;
using UnityEngine;

public class IdleState: IPlayerState
{
    private readonly PlayerStateManager _stateManger;
    private readonly Action<MovementInputEventArgs> _onMovementInput;
    private readonly Action<BufferedInputEventArgs> _onRollButtonPressed;

    private readonly Action<BufferedInputEventArgs> _onAtkMainPerformed;

    // private Vector2 _cachedMovement;
    // private bool _hasCachedMovement;


    public IdleState(PlayerStateManager manager)
    {
        _stateManger = manager;

        _onMovementInput = OnMovementInput;
        _onRollButtonPressed = OnRollButtonPressed;
        _onAtkMainPerformed = OnAtkmainPerformed;
    }

    public void Enter()
    {
        //stateManger.animator.SetFloat(stateManger.animatorMoveState, 0f);//, 0.1f, Time.deltaTime);
        //_stateManger.AnimationController.SmoothTransition(_stateManger.AnimationController.MoveStateHash, 0f, 0.1f);
        _stateManger.AnimSmoothTransition(AnimParams.MoveState, 0f, 0.1f);
        EventCenter.OnMovementInput += _onMovementInput;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed += _onAtkMainPerformed;
    }

    public void Update() 
    {

    }

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        //Debug.Log($"idlestate, {e}");
        InputBufferSystem.Instance.ConsumInputItem(e.InputUniqueId);
        EventCenter.PublishStateChange(PlayerStateType.Roll);
    }

    public void Exit()
    {
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed -= _onAtkMainPerformed;
    }

    private void OnMovementInput(MovementInputEventArgs e)
    {
        // _cachedMovement = e.Movement;
        // _hasCachedMovement = e.HasMovement;
        if (e.HasMovement)
        {
            EventCenter.PublishStateChange(PlayerStateType.Walk);
        }
    }

    private void OnAtkmainPerformed(BufferedInputEventArgs e)
    {
        _stateManger.CachedAtkType = AttackType.Light;
        Debug.Log("Atk light in idle");
        EventCenter.PublishStateChange(PlayerStateType.Attack);
    }
    // public void HandleMovement()
    // {
    //     if (stateManger.movementInput.magnitude > 0.1f)
    //     {
    //         stateManger.SwitchState(stateManger.walkState);
    //     }
    //     // Vector3 moveDir = stateManger.GetCameraRelativeMoveDirection();
    //     // Debug.Log(string.Format("in idle state, moveDir = {0}", moveDir));
    //     // if (moveDir.magnitude < 0.1f)
    //     //     return;

    //     // stateManger.FaceMoveDirection(moveDir);

    //     // stateManger.controller.Move(moveDir * stateManger.status.moveSpeed * Time.fixedDeltaTime);

    // }
    public void FixedUpdate()
    {
        // if (_hasCachedMovement)
        // {
        //     EventCenter.PublishMovementInput(_cachedMovement);
        //     _hasCachedMovement = false;
        // }
        //HandleMovement();
        //throw new System.NotImplementedException();
    }
}
