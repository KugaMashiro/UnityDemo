using System;
using Unity.VisualScripting;
using UnityEngine;

public class RunState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;
    private readonly Action<MovementInputEventArgs> _onMovementInput;
    private readonly Action _onRunButtonPressed;
    private readonly Action<BufferedInputEventArgs> _onRollButtonPressed;
    private readonly Action<BufferedInputEventArgs> _onAtkMainPerformed;
    private readonly Action<BufferedInputEventArgs> _onStrongAtkMainPerformed;
    private readonly Action<BufferedInputEventArgs> _onUseItemPressed;

    private Vector2 _cachedMovement;
    public RunState(PlayerStateManager manager)
    {
        _stateManager = manager;
        _onMovementInput = OnMovementInput;
        _onRunButtonPressed = OnRunButtunPressed;
        _onRollButtonPressed = OnRollButtonPressed;
        _onAtkMainPerformed = OnAtkmainPerformed;
        _onStrongAtkMainPerformed = OnStrongAtkmainPerformed;
        _onUseItemPressed = OnUseItemPressed;
    }

    public void Enter()
    {
        EventCenter.OnMovementInput += _onMovementInput;
        EventCenter.OnRunButtunPressed += _onRunButtonPressed;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed += _onAtkMainPerformed;
        EventCenter.OnStrongAttackMainPerformed += _onStrongAtkMainPerformed;
        EventCenter.OnUseItemPressed += _onUseItemPressed;

        _cachedMovement = _stateManager.MovementInput;
        float clampInput = 0.9f;//Mathf.Clamp(_cachedMovement.magnitude, 0.7f, 0.9f);
        _stateManager.AnimSmoothTransition(AnimParams.MoveState, clampInput, 0.1f);
        _stateManager.AnimController.SetAnimStateIndex(AnimStateIndex.Locomotion);
        _stateManager.AnimController.SetMotionType(PlayerMotionType.Run);
    }

    public void Update()
    {

    }

    public void Exit()
    {
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnRunButtunPressed -= _onRunButtonPressed;
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed -= _onAtkMainPerformed;
        EventCenter.OnStrongAttackMainPerformed -= _onStrongAtkMainPerformed;
        EventCenter.OnUseItemPressed -= _onUseItemPressed;

        //_stateManger.AnimController.SetMotionState(PlayerMotionType.Idle);
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
        Vector3 moveDir = _stateManager.GetCameraRelMoveDir(_cachedMovement, Camera.main.transform);
        //Debug.Log(string.Format("in walk state, moveDir = {0}", moveDir));
        // if (!MoveDirUtils.IsValidMoveDirection(moveDir))
        //     return;
        // if (moveDir.sqrMagnitude < 0.01f)
        //         return;

        _stateManager.Controller.ForceFace(moveDir);

        _stateManager.Controller.Move(moveDir, _stateManager.Status.RunSpeed, Time.fixedDeltaTime);
    }

    private void OnRunButtunPressed()
    {
        EventCenter.PublishStateChange(PlayerStateType.Walk);
    }

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);
        EventCenter.PublishStateChange(PlayerStateType.Roll);
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
    private void OnUseItemPressed(BufferedInputEventArgs e)
    {
        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);
        EventCenter.PublishStateChange(PlayerStateType.UseItem);
    }

    public void LateUpdate()
    {

    }

}