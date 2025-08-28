using System;
using Unity.VisualScripting;
using UnityEngine;

public class IdleState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;
    private readonly Action<MovementInputEventArgs> _onMovementInput;
    private readonly Action<BufferedInputEventArgs> _onRollButtonPressed;

    private readonly Action<BufferedInputEventArgs> _onAtkMainPerformed;
    private readonly Action<BufferedInputEventArgs> _onStrongAtkMainPerformed;

    // private Vector2 _cachedMovement;
    // private bool _hasCachedMovement;


    public IdleState(PlayerStateManager manager)
    {
        _stateManager = manager;

        _onMovementInput = OnMovementInput;
        _onRollButtonPressed = OnRollButtonPressed;
        _onAtkMainPerformed = OnAtkmainPerformed;
        _onStrongAtkMainPerformed = OnStrongAtkmainPerformed;
    }

    public void Enter()
    {
        Debug.Log("enter idle");
        //stateManger.animator.SetFloat(stateManger.animatorMoveState, 0f);//, 0.1f, Time.deltaTime);
        //_stateManger.AnimationController.SmoothTransition(_stateManger.AnimationController.MoveStateHash, 0f, 0.1f);
        _stateManager.AnimSmoothTransition(AnimParams.MoveState, 0f, 0.1f);
        _stateManager.AnimController.SetAnimStateIndex(AnimStateIndex.Locomotion);
        _stateManager.AnimController.SetMotionType(PlayerMotionType.Idle);

        EventCenter.OnMovementInput += _onMovementInput;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed += _onAtkMainPerformed;
        EventCenter.OnStrongAttackMainPerformed += _onStrongAtkMainPerformed;


        //EventCenter.OnHit += OnHit;
    }
    public void Exit()
    {
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed -= _onAtkMainPerformed;
        EventCenter.OnStrongAttackMainPerformed -= _onStrongAtkMainPerformed;

        //EventCenter.OnHit -= OnHit;
    }

    public void Update()
    {

    }

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        //Debug.Log($"idlestate, {e}");
        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);
        EventCenter.PublishStateChange(PlayerStateType.Roll);
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
        _stateManager.CachedAtkType = AttackType.Light;
        //Debug.Log("Atk light in idle");
        EventCenter.PublishStateChange(PlayerStateType.Attack);
    }

    private void OnStrongAtkmainPerformed(BufferedInputEventArgs e)
    {
        _stateManager.CachedAtkType = AttackType.Heavy;
        //Debug.Log("Atk light in idle");
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

    public void LateUpdate()
    {
        
    }


    // public void OnHit()
    // {
    //     EventCenter.PublishStateChange(PlayerStateType.Hit);
    // }
}
