

using UnityEngine;
using UnityEngine.InputSystem;


public enum AttackType
{
    None,
    Light,
    Heavy
}

public class PlayerInput : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    private PlayerStateManager _stateManager;
    //private InputBufferSystem _inputbuffer;
    private bool _isShiftPressed;

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
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMovePerformed;
        _inputActions.Player.Move.canceled -= OnMoveCanceled;
        _inputActions.Player.Run.performed -= OnRunPerformed;
        _inputActions.Player.Roll.performed -= OnRollPerformed;
        _inputActions.Player.AttackMain.performed -= OnAttackMainPerformed;

        _inputActions.Player.Disable();
    }

    #region InputHandler
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
        Vector3 bufferedRollDir = _stateManager.GetCameraRelMoveDir();
        uint bufferedInputId = InputBufferSystem.Instance.AddInput(BufferedInputType.Roll, bufferedRollDir);
        EventCenter.PublishRollButtonPressed(bufferedInputId);
    }

    private void OnAttackMainPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Attack Main Performed");
        Vector3 bufferedAtkDir = _stateManager.GetCameraRelMoveDir();
        uint bufferedInputId = InputBufferSystem.Instance.AddInput(BufferedInputType.AttackLight, bufferedAtkDir);
        EventCenter.PublishAtkMainPerformed(bufferedInputId);
    }
    #endregion
}