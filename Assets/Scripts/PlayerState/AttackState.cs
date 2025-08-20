using System;
using System.Collections.Generic;
using Cinemachine.Examples;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

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

    private readonly Action _onAnimComboWindowOpen;
    private readonly Action _onAnimComboWindowStart;
    private readonly Action _onAnimComboWindowEnd;
    private readonly Action _onAnimAtkStateTrans;

    private readonly Action<BufferedInputEventArgs> _onRollButtonPressed;
    private readonly Action<BufferedInputEventArgs> _onAttackMainPerformed;

    private List<float> MoveDis = new List<float> { 1f, 1f };
    //private float? _rootTZPercentage;

    private bool _canInteract;


    public AttackState(PlayerStateManager manager)
    {
        _stateManager = manager;
        _onAnimAtkEnd = OnAnimAtkEnd;
        _onAnimComboWindowStart = OnAnimComboWindowStart;
        _onAnimComboWindowEnd = OnAnimComboWindowEnd;
        _onAnimAtkStateTrans = OnAnimAtkStateTrans;

        _onRollButtonPressed = OnRollButtonPressed;
        _onAttackMainPerformed = OnAttackMainPerformed;
        _onAnimComboWindowOpen = OnAnimComboWindowOpen;
    }

    private void PrepareNextCombo()
    {
        _curComboStage++;
        _comboTriggeredFlag = false;
        _rootTZPercentage = null;
        _canInteract = false;
    }

    private void ClearComboState()
    {
        _curComboStage = 0;
        _comboTriggeredFlag = false;
        _rootTZPercentage = null;
        _canInteract = false;
    }

    public void Enter()
    {
        EventCenter.OnAnimAtkEnd += _onAnimAtkEnd;
        EventCenter.OnAnimComboWindowOpen += _onAnimComboWindowOpen;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed += _onAttackMainPerformed;

        if (_stateManager.CachedAtkType == AttackType.None)
        {
            Debug.LogError("Initial AttackState with None input!");
        }
        _curAtkType = _stateManager.CachedAtkType;
        _stateManager.CachedAtkType = AttackType.None;

        GetInitialDir();

        _stateManager.AnimController.SetInteger(AnimParams.ComboIndex, _curComboStage);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Atk);
    }
    public void Exit()
    {
        EventCenter.OnAnimAtkEnd -= _onAnimAtkEnd;
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;
        EventCenter.OnAnimComboWindowOpen -= _onAnimComboWindowOpen;
        EventCenter.OnAttackMainPerformed -= _onAttackMainPerformed;

        ClearComboState();

        _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_Atk);
        _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
    }

    private void GetInitialDir()
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

        if (MoveDirUtils.IsValidMoveDirection(_initialDir))
        {
            _stateManager.Controller.ForceFace(_initialDir);
        }
        else
        {
            _initialDir = _stateManager.Controller.GetCurrentFacing();
        }
    }

    private void Restart()
    {
        Debug.Log($"in restart, {_curComboStage}");
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_Atk);

        _stateManager.AnimController.SetInteger(AnimParams.ComboIndex, _curComboStage);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Atk);
    }

    private void OnAttackMainPerformed(BufferedInputEventArgs e)
    {
        Debug.Log("in Combo");
        if (!_canInteract) return;

        _comboTriggeredFlag = true;
        _stateManager.CachedAtkType = AttackType.Light;

        if (_curAtkType == AttackType.Light)
        {
            PrepareNextCombo();
            Restart();
        }
    }

    private void OnStrongAttackMainPressed(BufferedInputEventArgs e)
    {
        _comboTriggeredFlag = true;
        _stateManager.CachedAtkType = AttackType.Heavy;
    }

    private void OnAnimAtkEnd()
    {
        Debug.Log("anim atk end");
        ClearComboState();
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

    private void OnAnimComboWindowOpen()
    {
        Debug.Log("set _canInteract");
        _canInteract = true;

        
        InputBufferItem bufferedInput = _stateManager.GetValidInput(AllowedBufferedInputs);
        if (bufferedInput != null)
        {
            if (bufferedInput.InputType == BufferedInputType.Roll)
            {
                _stateManager.CachedDir = bufferedInput.BufferedDir;
                InputBufferSystem.Instance.ConsumInputItem(bufferedInput.UniqueId);

                ClearComboState();
                _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
                _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);

                EventCenter.PublishStateChange(PlayerStateType.Roll);
                return;
            }
            else if (bufferedInput.InputType == BufferedInputType.AttackLight ||
                bufferedInput.InputType == BufferedInputType.AttackHeavy)
            {
                _stateManager.CachedDir = bufferedInput.BufferedDir;
                InputBufferSystem.Instance.ConsumInputItem(bufferedInput.UniqueId);
                if (bufferedInput.InputType == BufferedInputType.AttackLight && _curAtkType == AttackType.Light)
                {
                    PrepareNextCombo();
                    Restart();
                }
                else if (bufferedInput.InputType == BufferedInputType.AttackHeavy && _curAtkType == AttackType.Heavy)
                {
                    PrepareNextCombo();
                    Restart();
                }
                else
                {
                    ClearComboState();
                    _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
                    _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);
                    _curAtkType = AttackType.Heavy;
                    Restart();
                }
            }
        }
    }

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        //Debug.Log($"_canInteract: {_canInteract}");
        if (!_canInteract) return;

        InputBufferSystem.Instance.ConsumInputItem(e.InputUniqueId);
        _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
        _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);

        ClearComboState();
        EventCenter.PublishStateChange(PlayerStateType.Roll);
    }


    public void FixedUpdate()
    {
        if (!_stateManager.AnimController.IsInTransition(1))
        {
            HandleMovement();
        }
    }

    public void Update()
    {

    }

    public void HandleMovement()
    {
        float curZPercentage = _stateManager.AnimController.GetFloat(AnimParams.RootZTransitionL1);

        if (_rootTZPercentage.HasValue)
        {
            //Debug.Log(curZPercentage - _rootTZPercentage.Value);
            _stateManager.Controller.Move(_initialDir,
                (curZPercentage - _rootTZPercentage.Value) * MoveDis[0]);
        }
        _rootTZPercentage = curZPercentage;
    }
}