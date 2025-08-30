using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;
    private AnimatorStateInfo _stateInfo;
    private Vector2 _cachedMovement;

    private Action<MovementInputEventArgs> _onMovementInput;
    private Action<BufferedInputEventArgs> _onRollButtonPressed;

    private bool _canInteract;
    private bool _haveHandlebuffer = false;
    private bool _haveComsumeItem = false;

    private bool _moveable;
    private bool _shouldLock;
    private float _moveSpeed;
    private float _moveBlendFactor;
    private bool _reuseable;

    private int _curHandlingItem;

    private List<BufferedInputType> AllowedBufferedInputs { get; }
        = new List<BufferedInputType> { BufferedInputType.Roll };
    private List<BufferedInputType> AllowedBufferedInputsReuse { get; }
        = new List<BufferedInputType> { BufferedInputType.Roll, BufferedInputType.UseItem };


    public UseItemState(PlayerStateManager manager)
    {
        _stateManager = manager;
        _onMovementInput = OnMovementInput;
        _onRollButtonPressed = OnRollButtonPressed;
    }

    public void Enter()
    {
        Debug.Log("enter useItem");
        EventCenter.OnMovementInput += _onMovementInput;
        EventCenter.OnRollButtonPressed += _onRollButtonPressed;

        _curHandlingItem = _stateManager.Inventory.CurrentItemIndex;

        _stateManager.AnimController.SetItemLayer(1f);
        _stateManager.AnimController.SetAnimStateIndex(AnimStateIndex.Locomotion);
        _stateManager.AnimController.SetMotionType(PlayerMotionType.Walk);
        // _cachedMovement = _stateManager.MovementInput;

        // SetWalkBlend();
        // _stateManager.AnimController.SetBool(AnimParams.IsItemValid, CheckItemValid());
        // _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_UseItem);
        // _stateManager.AnimController.SetTrigger(AnimParams.Trigger_UseItem);
        SetItemInfos();
        StartUseItem();

        //EventCenter.PublishStateChange(PlayerStateType.Idle);
    }

    public void Exit()
    {
        EventCenter.OnMovementInput -= _onMovementInput;
        EventCenter.OnRollButtonPressed -= _onRollButtonPressed;

        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_UseItem);
        _stateManager.AnimController.SetItemLayer(0f);
    }

    private void SetItemInfos()
    {
        ItemData curdata = _stateManager.GetCurItemData();
        _moveable = curdata.moveable;
        _shouldLock = curdata.shouldLock;
        _moveSpeed = curdata.moveSpeed;
        _moveBlendFactor = curdata.moveBlendFactor;
        _reuseable = curdata.reuseable;
    }

    private void ResetBools()
    {
        _canInteract = false;
    }

    private bool CheckItemValid()
    {
        return _stateManager.Inventory.CanConsumeItem();
    }
    // private void OnAnimUseItemEnd()
    // {
    //     EventCenter.PublishStateChange(PlayerStateType.Idle);
    // }

    private void OnMovementInput(MovementInputEventArgs e)
    {
        if (_moveable)
        {
            _cachedMovement = e.Movement;

            SetWalkBlend();
        }
        if (_canInteract)
        {
            _canInteract = false;
            if (e.HasMovement)
            {
                //Debug.Log("roll -> walk");
                EventCenter.PublishStateChange(PlayerStateType.Walk);
            }
        }
    }

    private void OnRollButtonPressed(BufferedInputEventArgs e)
    {
        if (!_canInteract) return;
        _canInteract = false;

        InputBufferSystem.Instance.ConsumeInputItem(e.InputUniqueId);
        // ClearComboState();
        EventCenter.PublishStateChange(PlayerStateType.Roll);
    }

    private void StartUseItem()
    {
        ResetBools();

        _cachedMovement = _stateManager.MovementInput;
        SetWalkBlend();
        _stateManager.AnimController.SetBool(AnimParams.IsItemValid, CheckItemValid());
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_UseItem);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_UseItem);
    }

    private void SetWalkBlend()
    {
        if (!MoveDirUtils.IsValidMoveDirection(_cachedMovement) || !_moveable)
        {
            _stateManager.AnimSmoothTransition(AnimParams.LockRelativeX, 0f,
                AnimParams.LockRelativeZ, 0f, 0.1f);
        }
        else
        {
            if (_stateManager.IsLocked && _shouldLock)
            {
                _stateManager.AnimSmoothTransition(AnimParams.LockRelativeX, _cachedMovement.x * _moveBlendFactor,
                AnimParams.LockRelativeZ, _cachedMovement.y * _moveBlendFactor, 0.1f);
            }
            else
            {
                _stateManager.AnimSmoothTransition(AnimParams.LockRelativeX, 0f,
                AnimParams.LockRelativeZ, _moveBlendFactor, 0.1f);
            }
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDir;
        if (!_stateManager.IsLocked)
        {
            moveDir = _stateManager.GetCameraRelMoveDir(_cachedMovement, Camera.main.transform);
        }
        else
        {
            moveDir = _stateManager.GetTargetRelMoveDir(_cachedMovement);
        }

        if (_stateManager.IsLocked && _shouldLock)
        {
            _stateManager.Controller.ForceFaceTarget(_stateManager.LockTargetTransform);
        }
        else
        {
            _stateManager.Controller.ForceFace(moveDir);
        }

        _stateManager.Controller.Move(moveDir, _moveSpeed, Time.fixedDeltaTime);
    }


    public void FixedUpdate()
    {
        if (_moveable)
        {
            HandleMovement();
        }
    }

    private void ItemEndTransition()
    {
        if (MoveDirUtils.IsValidMoveDirection(_stateManager.MovementInput))
        {
            EventCenter.PublishStateChange(PlayerStateType.Walk);
            return;
        }
        else
        {
            EventCenter.PublishStateChange(PlayerStateType.Idle);
            return;
        }
    }

    private void HandleBufferInput()
    {
        InputBufferItem bufferedInput;
        if (_reuseable)
        {
            bufferedInput = _stateManager.GetValidInput(AllowedBufferedInputsReuse);
        }
        else
        {
            bufferedInput = _stateManager.GetValidInput(AllowedBufferedInputs);
        }

        if (bufferedInput != null)
        {
            if (bufferedInput.InputType == BufferedInputType.Roll)
            {
                _canInteract = false;
                _stateManager.CacheDirAndComsumeInputBuffer(bufferedInput);
                EventCenter.PublishStateChange(PlayerStateType.Roll);
                return;
            }
            else if (bufferedInput.InputType == BufferedInputType.UseItem)
            {
                InputBufferSystem.Instance.ConsumeInputItem(bufferedInput.UniqueId);
                if (_stateManager.Inventory.CurrentItemIndex == _curHandlingItem)
                {
                    StartUseItem();
                }
                else
                {
                    EventCenter.PublishStateChange(PlayerStateType.UseItem);
                }
            }
        }
    }

    private void ConsumeItem()
    {
        _stateManager.Inventory.ConsumeItem();
    }

    public void LateUpdate()
    {
        _stateInfo = _stateManager.AnimItemLayerInfo();
        float normalizedTime = _stateInfo.normalizedTime;
        //Debug.Log($"{_stateInfo.shortNameHash == AnimStates.Drink}, normalized time: {normalizedTime}");
        if (_stateInfo.shortNameHash == AnimStates.Drink)
        {
            if (normalizedTime < 0.7f && _haveComsumeItem)
            {
                _haveComsumeItem = false;
            }
            if (normalizedTime > 0.7f && !_haveComsumeItem)
            {
                ConsumeItem();
                _haveComsumeItem = true;
            }

            if (normalizedTime < 0.8f && _haveHandlebuffer)
            {
                _haveHandlebuffer = false;
            }
            if (normalizedTime >= 0.8f && !_haveHandlebuffer)
            {
                _canInteract = true;
                HandleBufferInput();
                _haveHandlebuffer = true;
            }

            if (normalizedTime >= 0.99f)
            {
                ItemEndTransition();
                return;
            }

        }
        else if (_stateInfo.shortNameHash == AnimStates.DrinkNot)
        {
            if (normalizedTime < 0.8f && _haveHandlebuffer)
            {
                _haveHandlebuffer = false;
            }
            if (normalizedTime >= 0.8f && !_haveHandlebuffer)
            {
                _canInteract = true;
                HandleBufferInput();
                _haveHandlebuffer = true;
            }

            if (normalizedTime >= 0.99f)
            {
                ItemEndTransition();
                return;
            }

        }
    }
    public void Update()
    {
        
    }
}
