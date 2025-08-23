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
    #region CallBack Cache
    private readonly Action _onAnimAtkEnd;

    private readonly Action _onAnimInteractWindowOpen;
    private readonly Action _onAnimComboWindowStart;
    private readonly Action _onAnimComboWindowEnd;
    private readonly Action _onAnimAtkStateTrans;

    private readonly Action<BufferedInputEventArgs> _onRollButtonPressed;
    private readonly Action<BufferedInputEventArgs> _onAttackMainPerformed;
    private readonly Action _onAttackMainCanceled;
    private readonly Action _onAnimChargeStart;
    private readonly Action _onAnimChargeEnd;

    #endregion
    //private float? _rootTZPercentage;

    private bool _canInteract;
    private bool _chargable = false;
    private bool _hearingCancel = false;
    private bool _hasPendingCancel = false;
    private bool _isAtkTransitionPending = false;


    private List<float> MoveDis = new List<float> { 1f, 1f };
    private readonly Dictionary<AttackType, int> _maxComboCnt = new()
    {
        {AttackType.Light, 2},
        {AttackType.Heavy, 2},
    };

    private static readonly Dictionary<BufferedInputType, AttackType> InputToAttackTypeMap = new()
    {
        { BufferedInputType.AttackLight, AttackType.Light },
        { BufferedInputType.AttackHeavy, AttackType.Heavy }
    };

    private static readonly List<bool> ChargableList = new(){
        false, false,
    };

    public AttackState(PlayerStateManager manager)
    {
        _stateManager = manager;
        _onAnimAtkEnd = OnAnimAtkEnd;
        _onAnimComboWindowStart = OnAnimComboWindowStart;
        _onAnimComboWindowEnd = OnAnimComboWindowEnd;
        _onAnimAtkStateTrans = OnAnimAtkStateTrans;

        _onRollButtonPressed = OnRollButtonPressed;
        _onAttackMainPerformed = OnAttackMainPerformed;
        _onAttackMainCanceled = OnAttackMainCanceled;
        _onAnimInteractWindowOpen = OnAnimInteractWindowOpen;
        _onAnimChargeStart = OnAnimChargeStart;
        _onAnimChargeEnd = OnAnimChargeEnd;
    }

    private void PrepareNextCombo()
    {
        _curComboStage++;
        _curComboStage %= _maxComboCnt[_curAtkType];
        _comboTriggeredFlag = false;
        _rootTZPercentage = null;
        _canInteract = false;
        _hearingCancel = false;
        _hasPendingCancel = false;

        _chargable = ChargableList[_curComboStage];
    }

    private void ClearComboState()
    {
        _curComboStage = 0;
        _comboTriggeredFlag = false;
        _rootTZPercentage = null;
        _canInteract = false;
        _hearingCancel = false;
        _hasPendingCancel = false;
        _chargable = ChargableList[_curComboStage];
    }

    public void Enter()
    {
        EventCenter.OnAnimAtkEnd += _onAnimAtkEnd;
        EventCenter.OnAnimInteractWindowOpen += _onAnimInteractWindowOpen;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed += _onAttackMainPerformed;
        EventCenter.OnAttackMainCanceled += _onAttackMainCanceled;
        EventCenter.OnAnimChargeStart += _onAnimChargeStart;
        EventCenter.OnAnimChargeEnd += _onAnimChargeEnd;

        if (_stateManager.CachedAtkType == AttackType.None)
        {
            Debug.LogError("Initial AttackState with None input!");
        }
        _curAtkType = _stateManager.CachedAtkType;
        _stateManager.CachedAtkType = AttackType.None;

        _canInteract = false;
        _hasPendingCancel = false;
        _hearingCancel = false;
        _chargable = ChargableList[_curComboStage];

        _isAtkTransitionPending = true;

        GetInitialDir();

        _stateManager.AnimController.SetAnimStateIndex(AnimStateIndex.Attack);
        _stateManager.AnimController.SetInteger(AnimParams.AtkComboIndex, _curComboStage);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Atk);
        _stateManager.AnimController.SetBool(AnimParams.AtkChargable, _chargable);
    }
    public void Exit()
    {
        EventCenter.OnAnimAtkEnd -= _onAnimAtkEnd;
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;
        EventCenter.OnAnimInteractWindowOpen -= _onAnimInteractWindowOpen;
        EventCenter.OnAttackMainPerformed -= _onAttackMainPerformed;
        EventCenter.OnAttackMainCanceled -= _onAttackMainCanceled;
        EventCenter.OnAnimChargeStart -= _onAnimChargeStart;
        EventCenter.OnAnimChargeEnd -= _onAnimChargeEnd;

        ClearComboState();

        _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_Atk);
        _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_ChargeExit);
        //_stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
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
        _isAtkTransitionPending = true;
        Debug.Log($"in restart, {_curComboStage}");
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_Atk);
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_ChargeExit);

        _stateManager.AnimController.SetInteger(AnimParams.AtkComboIndex, _curComboStage);
        _stateManager.AnimController.SetBool(AnimParams.AtkChargable, _chargable);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Atk);
    }

    private void OnAttackMainPerformed(BufferedInputEventArgs e)
    {
        if (!_canInteract) return;
        Debug.Log("in Combo");

        _comboTriggeredFlag = true;
        _stateManager.CachedAtkType = AttackType.Light;

        if (_curAtkType == AttackType.Light)
        {
            PrepareNextCombo();
            Restart();
        }
    }

    private void OnAttackMainCanceled()
    {
        //Debug.Log("Attack Main Canceled");
        if (!_chargable) return;
        if (!_hearingCancel)
        {
            _hasPendingCancel = true;
            return;
        }
        else
        {
            Debug.Log("Charge break");
            TriggerChargeEnd();
        }
    }

    private void TriggerChargeEnd()
    {
        _hearingCancel = false;
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_ChargeExit);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_ChargeExit);
        Debug.Log("Charge End");
    }

    private void OnStrongAttackMainPressed(BufferedInputEventArgs e)
    {
        _comboTriggeredFlag = true;
        _stateManager.CachedAtkType = AttackType.Heavy;
    }

    private void OnAnimChargeStart()
    {
        _hearingCancel = true;
        if (_hasPendingCancel)
        {
            _hasPendingCancel = false;
            Debug.Log("Using Pending Cancel");
            TriggerChargeEnd();
        }
    }

    private void OnAnimChargeEnd()
    {
        Debug.Log("Max Charged");
        TriggerChargeEnd();
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

    private void TriggerExit()
    {
        _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
        _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);
    }

    private void HandleBufferedInput()
    {
        InputBufferItem bufferedInput = _stateManager.GetValidInput(AllowedBufferedInputs);
        if (bufferedInput != null)
        {
            if (bufferedInput.InputType == BufferedInputType.Roll)
            {
                // _stateManager.CachedDir = bufferedInput.BufferedDir;
                // InputBufferSystem.Instance.ConsumeInputItem(bufferedInput.UniqueId);
                _stateManager.CacheDirAndComsumeInputBuffer(bufferedInput);

                // ClearComboState();
                // _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
                // _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);
                TriggerExit();

                EventCenter.PublishStateChange(PlayerStateType.Roll);
                return;
            }
            else if (InputToAttackTypeMap.TryGetValue(bufferedInput.InputType, out var inputAtkType))
            {
                _stateManager.CacheDirAndComsumeInputBuffer(bufferedInput);
                if (inputAtkType == _curAtkType)
                {
                    PrepareNextCombo();
                    Restart();
                }
                else
                {
                    // ClearComboState();
                    // _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
                    // _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);
                    TriggerExit();
                    _curAtkType = inputAtkType;
                    Restart();
                }
            }
            // else if (bufferedInput.InputType == BufferedInputType.AttackLight ||
            //     bufferedInput.InputType == BufferedInputType.AttackHeavy)
            // {
            //     // _stateManager.CachedDir = bufferedInput.BufferedDir;
            //     // InputBufferSystem.Instance.ConsumeInputItem(bufferedInput.UniqueId);
            //     _stateManager.CacheDirAndComsumeInputBuffer(bufferedInput);
            //     if (bufferedInput.InputType == BufferedInputType.AttackLight && _curAtkType == AttackType.Light)
            //     {
            //         PrepareNextCombo();
            //         Restart();
            //     }
            //     else if (bufferedInput.InputType == BufferedInputType.AttackHeavy && _curAtkType == AttackType.Heavy)
            //     {
            //         PrepareNextCombo();
            //         Restart();
            //     }
            //     else
            //     {
            //         ClearComboState();
            //         _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
            //         _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);
            //         _curAtkType = AttackType.Heavy;
            //         Restart();
            //     }
            // }
        }
    }

    private void OnAnimInteractWindowOpen()
    {
        Debug.Log("set _canInteract");
        _canInteract = true;

        HandleBufferedInput();
    }

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        if (!_canInteract) return;
        Debug.Log($"RollButton callback, _canInteract: {_canInteract}");

        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);

        // _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
        // _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);
        // Debug.Log("Reset atkexit");
        TriggerExit();

        // ClearComboState();
        EventCenter.PublishStateChange(PlayerStateType.Roll);
    }


    public void FixedUpdate()
    {
        bool isInTransition = _stateManager.AnimController.IsInTransition(1);
        if (isInTransition || _isAtkTransitionPending)
        {
            if (isInTransition)
            {
                _isAtkTransitionPending = false;
            }
            return;
        }
        HandleMovement();
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