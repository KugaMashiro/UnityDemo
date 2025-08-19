using System;
using System.Collections.Generic;
using Cinemachine.Examples;
using UnityEngine;

public class AttackState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;

    private Vector3 _initialDir;
    private AttackType _curAtkType;
    private int _curComboStage;

    private float? _rootTZPercentage;
    private bool _comboTriggeredFlag;

    private List<BufferedInputType> AllowedBufferedInputs { get; }
        = new List<BufferedInputType> { BufferedInputType.AttackLight, BufferedInputType.Roll, BufferedInputType.AttackHeavy };
    //private 
    private readonly Action _onAnimAtkEnd;
    private readonly Action _onAnimComboWindowStart;
    private readonly Action _onAnimComboWindowEnd;
    private readonly Action _onAnimAtkStateTrans;


    public AttackState(PlayerStateManager manager)
    {
        _stateManager = manager;
        _onAnimAtkEnd = OnAnimAtkEnd;
        _onAnimComboWindowStart = OnAnimComboWindowStart;
        _onAnimComboWindowEnd = OnAnimComboWindowEnd;
        _onAnimAtkStateTrans = OnAnimAtkStateTrans;
    }

    private void PrepareNextCombo()
    {
        _curComboStage++;
        _comboTriggeredFlag = false;
    }

    private void ClearComboState()
    {
        _curComboStage = 0;
        _comboTriggeredFlag = false;
    }

    public void Enter()
    {
        EventCenter.OnAnimAtkEnd += _onAnimAtkEnd;

        if (_stateManager.CachedAtkType == AttackType.None)
        {
            Debug.LogError("Initial AttackState with None input!");
        }
        _curAtkType = _stateManager.CachedAtkType;
        _stateManager.CachedAtkType = AttackType.None;

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
            _stateManager.Controller.ForceFace(_initialDir);
        }
        else
        {
            _initialDir = _stateManager.Controller.GetCurrentFacing();
        }

        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Atk);
    }

    private void OnAttackMainPressed(BufferedInputEventArgs e)
    {
        _comboTriggeredFlag = true;
        _stateManager.CachedAtkType = AttackType.Light;

    }

    private void OnStrongAttackMainPressed(BufferedInputEventArgs e)
    {
        _comboTriggeredFlag = true;
        _stateManager.CachedAtkType = AttackType.Heavy;
    }

    private void OnAnimAtkEnd()
    {
        Debug.Log("anim atk end");
        EventCenter.PublishStateChange(PlayerStateType.Idle);
        // ClearComboState();
        // InputBufferItem bufferedInput = _stateManager.GetValidInput(AllowedBufferedInputs);
        // if (bufferedInput != null)
        // {
        //     if (bufferedInput.InputType == BufferedInputType.AttackLight
        //         || bufferedInput.InputType == BufferedInputType.AttackHeavy)
        //     {
        //         _stateManager.CachedDir = bufferedInput.BufferedDir;
        //         _stateManager.CachedAtkType = bufferedInput.InputType == BufferedInputType.AttackLight ? AttackType.Light : AttackType.Heavy;
        //         InputBufferSystem.Instance.ConsumInputItem(bufferedInput.UniqueId);
        //         EventCenter.PublishStateChange(PlayerStateType.Attack);
        //         return;
        //     }
        //     else if (bufferedInput.InputType == BufferedInputType.Roll)
        //     {
        //         _stateManager.CachedDir = bufferedInput.BufferedDir;
        //         InputBufferSystem.Instance.ConsumInputItem(bufferedInput.UniqueId);
        //         EventCenter.PublishStateChange(PlayerStateType.Roll);
        //         return;
        //     }
        // }
    }

    private void OnAnimComboWindowStart()
    {
        
    }

    private void OnAnimComboWindowEnd()
    {
        
    }

    private void OnAnimAtkStateTrans()
    {
        
    }

    public void Exit()
    {
        EventCenter.OnAnimAtkEnd -= _onAnimAtkEnd;
        _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_Atk);
    }

    public void FixedUpdate()
    {
        
    }

    public void Update()
    {
        
    }
}