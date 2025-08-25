using System;
using System.Collections.Generic;
using Cinemachine.Examples;
using Unity.VisualScripting;
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
    //private bool _comboTriggeredFlag;

    private List<BufferedInputType> AllowedBufferedInputs { get; }
        = new List<BufferedInputType> { BufferedInputType.AttackLight, BufferedInputType.Roll, BufferedInputType.AttackHeavy };
    //private 
    #region CallBack Cache
    private readonly Action _onAnimAtkEnd;
    private readonly Action _onAnimChargeStart;
    private readonly Action _onAnimChargeEnd;
    private readonly Action _onAnimInteractWindowOpen;
    private readonly Action _onAnimRotateWindowOpen;
    private readonly Action _onAnimRotateWindowClose;
    private readonly Action _onAnimMoveWindowOpen;
    // private readonly Action _onAnimComboWindowStart;
    // private readonly Action _onAnimComboWindowEnd;
    // private readonly Action _onAnimAtkStateTrans;

    private readonly Action<BufferedInputEventArgs> _onRollButtonPressed;
    private readonly Action<BufferedInputEventArgs> _onAttackMainPerformed;
    private readonly Action _onAttackMainCanceled;
    private readonly Action<BufferedInputEventArgs> _onStrongAttackMainPerformed;
    private readonly Action _onStrongAttackMainCanceled;
    private readonly Action<MovementInputEventArgs> _onMovementInput;

    #endregion
    //private float? _rootTZPercentage;

    private bool _canInteract = false;
    private bool _chargable = false;
    private bool _hearingCancel = false;
    private bool _hasPendingCancel = false;
    private bool _isAtkTransitionPending = false;
    private bool _canRotate = false;
    private bool _canMove = false;


    private readonly Dictionary<AttackType, List<float>> MoveDis = new()
    {
        { AttackType.Light, new List<float> { 2f, 1.5f }},
        { AttackType.Heavy, new List<float> { 1.2f } }
    };
    private readonly Dictionary<AttackType, int> _maxComboCnt = new()
    {
        {AttackType.Light, 2},
        {AttackType.Heavy, 1},
    };

    // private static readonly Dictionary<BufferedInputType, AttackType> InputToAttackTypeMap = new()
    // {
    //     { BufferedInputType.AttackLight, AttackType.Light },
    //     { BufferedInputType.AttackHeavy, AttackType.Heavy }
    // };

    private static readonly Dictionary<AttackType, List<bool>> ChargableList = new(){
        { AttackType.Light, new List<bool> {false, false } },
        {AttackType.Heavy, new List<bool>{ true} },
    };

    private static readonly Dictionary<AttackType, List<float>> RotateSpeed = new(){
        { AttackType.Light, new List<float> {5f, 5f } },
        {AttackType.Heavy, new List<float>{ 2f } },
    };

    public AttackState(PlayerStateManager manager)
    {
        _stateManager = manager;

        _onAnimAtkEnd = OnAnimAtkEnd;
        // _onAnimComboWindowStart = OnAnimComboWindowStart;
        // _onAnimComboWindowEnd = OnAnimComboWindowEnd;
        // _onAnimAtkStateTrans = OnAnimAtkStateTrans;
        _onAnimInteractWindowOpen = OnAnimInteractWindowOpen;
        _onAnimChargeStart = OnAnimChargeStart;
        _onAnimChargeEnd = OnAnimChargeEnd;
        _onAnimRotateWindowOpen = OnAnimRotateWindowOpen;
        _onAnimRotateWindowClose = OnAnimRotateWindowClose;
        _onAnimMoveWindowOpen = OnAnimMoveWindowOpen;

        _onRollButtonPressed = OnRollButtonPressed;
        _onAttackMainPerformed = OnAttackMainPerformed;
        _onAttackMainCanceled = OnAttackMainCanceled;
        _onStrongAttackMainPerformed = OnStrongAttackMainPerformed;
        _onStrongAttackMainCanceled = OnStrongAttackMainCanceled;
        _onMovementInput = OnMovementInput;
    }

    private void TransToNextCombo(bool hasPendingCancel=false)
    {
        _curComboStage++;
        _curComboStage %= _maxComboCnt[_curAtkType];
        //_comboTriggeredFlag = false;
        // _rootTZPercentage = null;
        // _canInteract = false;
        // _hearingCancel = false;
        // _hasPendingCancel = false;
        ClearAttackStatus();
        GetAndSetChargable();

        if (hasPendingCancel) _hasPendingCancel = true;

        Restart();
        // _chargable = ChargableList[_curAtkType][_curComboStage];
        // _stateManager.AnimController.SetBool(AnimParams.AtkChargable, _chargable);
    }

    private void TransToAnotherAtkType(AttackType nextType, bool hasPendingCancel=false)
    {
        _curAtkType = nextType;
        _curComboStage = 0;
        //_comboTriggeredFlag = false;
        // _rootTZPercentage = null;
        // _canInteract = false;
        // _hearingCancel = false;
        // _hasPendingCancel = false;
        ClearAttackStatus();
        SetAtkType();
        GetAndSetChargable();

        if (hasPendingCancel) _hasPendingCancel = true;

        Restart();
        // _chargable = ChargableList[_curAtkType][_curComboStage];
        // _stateManager.AnimController.SetAtkType(_curAtkType);
        // _stateManager.AnimController.SetBool(AnimParams.AtkChargable, _chargable);
    }

    private void ClearAttackStatus()
    {
        _rootTZPercentage = null;
        _canInteract = false;
        _canRotate = false;
        _canMove = false;
        _hearingCancel = false;
        _hasPendingCancel = false;
    }

    private void SetAtkType()
    {
        _stateManager.AnimController.SetAtkType(_curAtkType);
    }

    private void GetAndSetChargable()
    {
        _chargable = ChargableList[_curAtkType][_curComboStage];
        _stateManager.AnimController.SetBool(AnimParams.AtkChargable, _chargable);
    }

    public void Enter()
    {
        EventCenter.OnAnimAtkEnd += _onAnimAtkEnd;
        EventCenter.OnAnimInteractWindowOpen += _onAnimInteractWindowOpen;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;
        EventCenter.OnAttackMainPerformed += _onAttackMainPerformed;
        EventCenter.OnAttackMainCanceled += _onAttackMainCanceled;
        EventCenter.OnStrongAttackMainPerformed += _onStrongAttackMainPerformed;
        EventCenter.OnStrongAttackMainCanceled += _onStrongAttackMainCanceled;
        EventCenter.OnAnimChargeStart += _onAnimChargeStart;
        EventCenter.OnAnimChargeEnd += _onAnimChargeEnd;
        EventCenter.OnAnimRotateWindowOpen += _onAnimRotateWindowOpen;
        EventCenter.OnAnimRotateWindowClose += _onAnimRotateWindowClose;
        EventCenter.OnAnimMoveWindowOpen += _onAnimMoveWindowOpen;
        
        EventCenter.OnMovementInput += _onMovementInput;

        if (_stateManager.CachedAtkType == AttackType.None)
        {
            Debug.LogError("Initial AttackState with None input!");
        }
        _curAtkType = _stateManager.CachedAtkType;
        _stateManager.CachedAtkType = AttackType.None;
        GetInitialDir();

        _curComboStage = 0;
        _chargable = ChargableList[_curAtkType][_curComboStage];

        ClearAttackStatus();

        _isAtkTransitionPending = true;

        _stateManager.AnimController.SetAnimStateIndex(AnimStateIndex.Attack);
        _stateManager.AnimController.SetInteger(AnimParams.AtkComboIndex, _curComboStage);
        _stateManager.AnimController.SetAtkType(_curAtkType);
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
        EventCenter.OnStrongAttackMainPerformed -= _onStrongAttackMainPerformed;
        EventCenter.OnStrongAttackMainCanceled -= _onStrongAttackMainCanceled;
        EventCenter.OnAnimChargeStart -= _onAnimChargeStart;
        EventCenter.OnAnimChargeEnd -= _onAnimChargeEnd;
        EventCenter.OnAnimRotateWindowOpen -= _onAnimRotateWindowOpen;
        EventCenter.OnAnimRotateWindowClose -= _onAnimRotateWindowClose;
        EventCenter.OnAnimMoveWindowOpen -= _onAnimMoveWindowOpen;
        EventCenter.OnMovementInput -= _onMovementInput;

        ClearAttackStatus();

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
        //_initialDir = _stateManager.Controller.GetCurrentFacing();
    }

    private void Restart()
    {
        GetInitialDir();
        _isAtkTransitionPending = true;
        //Debug.Log($"in restart, {_curComboStage}");
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_Atk);
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_ChargeExit);

        _stateManager.AnimController.SetInteger(AnimParams.AtkComboIndex, _curComboStage);
        _stateManager.AnimController.SetBool(AnimParams.AtkChargable, _chargable);
        //_stateManager.AnimController.SetAtkType()
        _stateManager.AnimController.SetAtkType(_curAtkType);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Atk);
    }

    private void OnAttackMainPerformed(BufferedInputEventArgs e)
    {
        if (!_canInteract) return;
        Debug.Log($"in Combo Light, {_curAtkType}");
        _canInteract = false;

        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);
        // _comboTriggeredFlag = true;
        // _stateManager.CachedAtkType = AttackType.Light;

        if (_curAtkType == AttackType.Light)
        {
            TransToNextCombo();
            //Restart();
        }
        else if (_curAtkType == AttackType.Heavy)
        {
            //_curAtkType = AttackType.Light;
            TransToAnotherAtkType(AttackType.Light);
            //Restart();
        }
    }

    private void OnAttackMainCanceled()
    {
        //Debug.Log("Attack Main Canceled");
        if (!_chargable) return;
        if (_curAtkType != AttackType.Light) return;
        if (!_hearingCancel)
        {
            _hasPendingCancel = true;
            return;
        }
        else
        {
            Debug.Log("Light Charge break");
            TriggerChargeEnd();
        }
    }

    private void OnStrongAttackMainPerformed(BufferedInputEventArgs e)
    {
        if (!_canInteract) return;
        _canInteract = false;
        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);

        if (_curAtkType == AttackType.Heavy)
        {
            TransToNextCombo();
            //Restart();
        }
        else if (_curAtkType == AttackType.Light)
        {
            //_curAtkType = AttackType.Heavy;
            TransToAnotherAtkType(AttackType.Heavy);
            //Restart();
        }
    }

    private void OnStrongAttackMainCanceled()
    {
        //Debug.Log("Attack Main Canceled");
        if (!_chargable) return;
        if (_curAtkType != AttackType.Heavy) return;
        if (!_hearingCancel)
        {
            _hasPendingCancel = true;
            return;
        }
        else
        {
            Debug.Log("Heavy Charge break");
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

    // private void OnStrongAttackMainPressed(BufferedInputEventArgs e)
    // {
    //     _comboTriggeredFlag = true;
    //     _stateManager.CachedAtkType = AttackType.Heavy;
    // }

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
        //TransToAnotherAtkType();
        ClearAttackStatus();
        //TriggerExit();
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



    // private void OnAnimComboWindowStart()
    // {

    // }

    // private void OnAnimComboWindowEnd()
    // {

    // }

    // private void OnAnimAtkStateTrans()
    // {

    // }

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
                _canInteract = false;
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
            else if (InputToAttackTypeMap.TryGet(bufferedInput.InputType, out var inputAtkType))
            {
                _canInteract = false;
                _stateManager.CacheDirAndComsumeInputBuffer(bufferedInput);
                bool inputCanceled = bufferedInput.ReleaseTime.HasValue;
                Debug.Log($"Using Buffered AtkInput, canceled: {inputCanceled}");
                if (inputAtkType == _curAtkType)
                {
                    TransToNextCombo(inputCanceled);
                    //Restart();
                }
                else
                {
                    // ClearComboState();
                    // _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
                    // _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);
                    //TriggerExit();
                    //_curAtkType = inputAtkType;
                    TransToAnotherAtkType(inputAtkType, inputCanceled);
                    //Restart();
                }
                return;
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

    private void OnAnimMoveWindowOpen()
    {
        _canMove = true;
        Debug.Log("set canMove");
        if (MoveDirUtils.IsValidMoveDirection(_stateManager.MovementInput))
        {
            _canInteract = false;
            TriggerExit();
            EventCenter.PublishStateChange(PlayerStateType.Walk);
            //return;
        }
    }

    private void OnAnimRotateWindowOpen()
    {
        _canRotate = true;
    }

    private void OnAnimRotateWindowClose()
    {
        _canRotate = false;
    }

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        if (!_canInteract) return;
        Debug.Log($"RollButton callback, _canInteract: {_canInteract}");
        _canInteract = false;

        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);

        // _stateManager.AnimController.Animator.ResetTrigger(AnimParams.Trigger_AtkExit);
        // _stateManager.AnimController.Animator.SetTrigger(AnimParams.Trigger_AtkExit);
        // Debug.Log("Reset atkexit");
        TriggerExit();

        // ClearComboState();
        EventCenter.PublishStateChange(PlayerStateType.Roll);
    }

    private void OnMovementInput(MovementInputEventArgs e)
    {
        if (!_canInteract) return;
        if (!_canMove) return;
        
        TriggerExit();
        EventCenter.PublishStateChange(PlayerStateType.Walk);
        
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
        HandleRotation();
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
                (curZPercentage - _rootTZPercentage.Value) * MoveDis[_curAtkType][_curComboStage]);
        }
        _rootTZPercentage = curZPercentage;
    }

    public void HandleRotation()
    {
        if (!_canRotate) return;
        Vector3 moveDir = _stateManager.GetCameraRelMoveDir();
        _stateManager.Controller.Face(moveDir, RotateSpeed[_curAtkType][_curComboStage], Time.fixedDeltaTime);
        _initialDir = _stateManager.Controller.GetCurrentFacing();
    }
}