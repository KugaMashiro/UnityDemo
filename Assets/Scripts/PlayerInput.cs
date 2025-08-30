

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public enum AttackType
{
    None = 0,
    Light = 1,
    Heavy = 2
}

public static class InputToAttackTypeMap
{
    private static readonly Dictionary<BufferedInputType, AttackType> Mapping = new()
    {
        { BufferedInputType.AttackLight, AttackType.Light },
        { BufferedInputType.AttackHeavy, AttackType.Heavy }
    };

    public static bool TryGet(BufferedInputType inputType, out AttackType atkType)
    {
        return Mapping.TryGetValue(inputType, out atkType);
    }
}

public class PlayerInput : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    private PlayerStateManager _stateManager;
    //private InputBufferSystem _inputbuffer;
    private bool _isShiftPressed;
    private bool _isAtkMain;
    private bool _isStrongAtkMain;
    private uint? _cachedAtkMainInput;
    private uint? _cachedStrongAtkMainInput;

    private void Awake()
    {
        //_isShiftPressed = false;
        _inputActions = new PlayerInputActions();
        _stateManager = GetComponent<PlayerStateManager>();
        //_inputbuffer = GetComponent<InputBufferSystem>();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();

        _inputActions.Player.Move.performed += OnMovePerformed;
        _inputActions.Player.Move.canceled += OnMoveCanceled;
        _inputActions.Player.Run.performed += OnRunPerformed;
        _inputActions.Player.Roll.performed += OnRollPerformed;
        _inputActions.Player.AttackMain.performed += OnAttackMainPerformed;
        _inputActions.Player.AttackMain.canceled += OnAttackMainCanceled;
        _inputActions.Player.StrongAttackMain.performed += OnStrongAttackMainPerformed;
        _inputActions.Player.StrongAttackMain.canceled += OnStrongAttackMainCanceled;
        _inputActions.Player.Shift.performed += OnShiftPressed;
        _inputActions.Player.Shift.canceled += OnShiftCanceled;

        _inputActions.Player.LockOn.performed += OnLockOnPressed;
        _inputActions.Player.UseItem.performed += OnUseItemPressed;
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMovePerformed;
        _inputActions.Player.Move.canceled -= OnMoveCanceled;
        _inputActions.Player.Run.performed -= OnRunPerformed;
        _inputActions.Player.Roll.performed -= OnRollPerformed;
        _inputActions.Player.AttackMain.performed -= OnAttackMainPerformed;
        _inputActions.Player.AttackMain.canceled -= OnAttackMainCanceled;
        _inputActions.Player.StrongAttackMain.performed -= OnStrongAttackMainPerformed;
        _inputActions.Player.StrongAttackMain.canceled -= OnStrongAttackMainCanceled;
        _inputActions.Player.Shift.performed -= OnShiftPressed;
        _inputActions.Player.Shift.canceled -= OnShiftCanceled;

        _inputActions.Player.LockOn.performed -= OnLockOnPressed;
        _inputActions.Player.UseItem.performed -= OnUseItemPressed;

        _inputActions.Player.Disable();
    }

    #region InputHandler
    private void OnUseItemPressed(InputAction.CallbackContext context)
    {
        Vector3 bufferedUseItemDir = _stateManager.GetRelMoveDir();
        uint bufferedInputId = InputBufferSystem.Instance.AddInput(BufferedInputType.UseItem, bufferedUseItemDir);
        EventCenter.PublishUseItem(bufferedInputId);
    }

    private void OnLockOnPressed(InputAction.CallbackContext context)
    {
        if (_stateManager.LockOnSystem.IsLocked)
        {
            _stateManager.LockOnSystem.UnLock();
            EventCenter.PublishLockOnCanceled();
        }
        else
        {
            if (_stateManager.LockOnSystem.TryLock())
            {
                EventCenter.PublishLockOnSucceed();
            }
        }
    }

    private void OnTryHit(InputAction.CallbackContext context)
    {
        EventCenter.PublishHit();
    }
    private void OnShiftPressed(InputAction.CallbackContext context)
    {
        _isShiftPressed = true;
    }

    private void OnShiftCanceled(InputAction.CallbackContext context)
    {
        _isShiftPressed = false;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // var args = EventPoolManager.Instance.GetPool<MovementInputEventArgs>().Get();
        // args.SetMovement(context.ReadValue<Vector2>());
        //Debug.Log(string.Format("Move Performed in PlayerInput, value is {0}", context.ReadValue<Vector2>()));
        EventCenter.PublishMovementInput(context.ReadValue<Vector2>());
        //EventPoolManager.Instance.GetPool<MovementInputEventArgs>().Release(args);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // var args = EventPoolManager.Instance.GetPool<MovementInputEventArgs>().Get();
        // args.Reset();
        EventCenter.PublishMovementInput(Vector2.zero);
        //EventPoolManager.Instance.GetPool<MovementInputEventArgs>().Release(args);
    }

    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        //Debug.Log($"run button pressed");
        EventCenter.PublishRunButtonPressed();
    }

    private void OnRollPerformed(InputAction.CallbackContext context)
    {
        // Debug.Log($"roll button pressed");
        // Animator tmp = GetComponentInChildren<Animator>();
        // //tmp.SetBool(AnimParams.IsJumpBack, false);
        // tmp.SetTrigger(AnimParams.Trigger_Roll);
        Vector3 bufferedRollDir = _stateManager.GetRelMoveDir();
        uint bufferedInputId = InputBufferSystem.Instance.AddInput(BufferedInputType.Roll, bufferedRollDir);
        EventCenter.PublishRollButtonPressed(bufferedInputId);
    }

    private void OnAttackMainPerformed(InputAction.CallbackContext context)
    {
        if (_isShiftPressed) return;
        if (_isStrongAtkMain) return;
        _isAtkMain = true;
        Debug.Log("Attack Main Performed");
        Vector3 bufferedAtkDir = _stateManager.GetRelMoveDir();
        uint bufferedInputId = InputBufferSystem.Instance.AddInput(BufferedInputType.AttackLight, bufferedAtkDir);
        if (_cachedAtkMainInput.HasValue)
        {
            Debug.LogError("Last AtkMain Didn't consumed!");
        }
        else
        {
            _cachedAtkMainInput = bufferedInputId;
        }

        EventCenter.PublishAtkMainPerformed(bufferedInputId);
    }

    private void OnAttackMainCanceled(InputAction.CallbackContext context)
    {
        if (_isShiftPressed) return;
        _isAtkMain = false;

        if (!_cachedAtkMainInput.HasValue)
        {
            //Debug.LogError("Last AtkMain Didn't cached!");
            return;
        }
        else
        {
            InputBufferSystem.Instance.SetReleaseTime(_cachedAtkMainInput.Value);
            _cachedAtkMainInput = null;
        }
        EventCenter.PublishAtkMainCanceled();
    }

    private void OnStrongAttackMainPerformed(InputAction.CallbackContext context)
    {
        if (_isAtkMain) return;
        _isStrongAtkMain = true;
        Debug.Log("Strong Attack Main Performed");
        Vector3 bufferedAtkDir = _stateManager.GetRelMoveDir();
        uint bufferedInputId = InputBufferSystem.Instance.AddInput(BufferedInputType.AttackHeavy, bufferedAtkDir);
        if (_cachedStrongAtkMainInput.HasValue)
        {
            Debug.LogError("Last StrongAtkMain Didn't consumed!");
        }
        else
        {
            _cachedStrongAtkMainInput = bufferedInputId;
        }

        EventCenter.PublishStrongAtkMainPerformed(bufferedInputId);
    }

    private void OnStrongAttackMainCanceled(InputAction.CallbackContext context)
    {
        _isStrongAtkMain = false;
        Debug.Log("Strong AtkMain Canceled");
        if (!_cachedStrongAtkMainInput.HasValue)
        {
            //Debug.LogError("Last StrongAtkMain Didn't cached!");
            return;
        }
        else
        {
            InputBufferSystem.Instance.SetReleaseTime(_cachedStrongAtkMainInput.Value);
            _cachedStrongAtkMainInput = null;
        }
        EventCenter.PublishStrongAtkMainCanceled();
    }
    #endregion
}