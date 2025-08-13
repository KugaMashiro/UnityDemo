using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class ThirdPersonController : MonoBehaviour
{
    //[Header("move parameters")]

    // [SerializeField]
    // private float moveSpeed = 5f;
    // [SerializeField]
    // private float rotationSpeed = 100f;

    [Header("camera settings")]
    public CinemachineVirtualCamera virtualCamera;
    // private CharacterController characterController;
    // private Vector2 moveInput;
    // private Vector2 rotateInput;
    // private Vector3 moveDir;
    // private Transform cameraTransform;
    //private Camera mainCamera;

    private PlayerInputActions playerInput;
    private bool isShiftPressed;

    // [HideInInspector]
    // public AnimateHandler animateHandler;

    [HideInInspector]
    private PlayerStateManager stateManager;



    private void Awake()
    {
        //characterController = GetComponent<CharacterController>();
        //animateHandler = GetComponentInChildren<AnimateHandler>();

        stateManager = GetComponent<PlayerStateManager>();
        //animateHandler.Init();

        //cameraTransform = Camera.main.transform;//virtualCamera.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isShiftPressed = false;

        playerInput = new PlayerInputActions();
        // playerInput.Player.Enable();
        // playerInput.Player.Move.performed += OnMove;
        // playerInput.Player.Move.canceled += OnMoveCanceled;
        // playerInput.Player.Look.performed += OnRotate;

        // playerInput.Player.StrongAttackMain.performed += OnStrongAttackMain;
        // playerInput.Player.AttackMain.performed += OnAttackMain;

        // playerInput.Player.SwitchUp.performed += OnSwitchUp;
        // playerInput.Player.SwitchLeft.performed += OnSwitchLeft;

        // playerInput.Player.Shift.performed += OnShiftPressed;
        // playerInput.Player.Shift.canceled += OnShiftCanceled;
    }

    private void OnEnable() 
    {
        //animateHandler.Init();
        playerInput.Player.Enable();
        playerInput.Player.Move.performed += OnMove;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Look.performed += OnRotate;

        playerInput.Player.StrongAttackMain.performed += OnStrongAttackMain;
        playerInput.Player.AttackMain.performed += OnAttackMain;

        playerInput.Player.SwitchUp.performed += OnSwitchUp;
        playerInput.Player.SwitchLeft.performed += OnSwitchLeft;

        playerInput.Player.Shift.performed += OnShiftPressed;
        playerInput.Player.Shift.canceled += OnShiftCanceled;
    }


    private void OnDisable()
    {
        Debug.Log("destory");
        playerInput.Player.Move.performed -= OnMove;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Look.performed -= OnRotate;

        playerInput.Player.StrongAttackMain.performed -= OnStrongAttackMain;
        playerInput.Player.AttackMain.performed -= OnAttackMain;

        playerInput.Player.SwitchUp.performed -= OnSwitchUp;
        playerInput.Player.SwitchLeft.performed -= OnSwitchLeft;

        playerInput.Player.Shift.performed -= OnShiftPressed;
        playerInput.Player.Shift.canceled -= OnShiftCanceled;

        playerInput.Player.Disable();
    }

    #region InputHandler
    private void OnShiftPressed(InputAction.CallbackContext context)
    {
        isShiftPressed = true;
    }

    private void OnShiftCanceled(InputAction.CallbackContext context)
    {
        isShiftPressed = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        //Debug.Log(context);
        //moveInput = context.ReadValue<Vector2>();
        stateManager.SetMoveInput(context.ReadValue<Vector2>());
    }

    public void OnMoveCanceled(InputAction.CallbackContext context)
    {
        //moveInput = Vector2.zero;
        stateManager.ResetMoveInput();
    }


    public void OnRotate(InputAction.CallbackContext context)
    {
        //Debug.Log(context);
        //rotateInput = context.ReadValue<Vector2>();
    }

    public void OnStrongAttackMain(InputAction.CallbackContext context)
    {   
        Debug.Log(context);
    }

    public void OnAttackMain(InputAction.CallbackContext context)
    {
        if (!isShiftPressed)
        {
            //Debug.Log(isShiftPressed);
            Debug.Log(context);
        }
    }

    public void OnSwitchUp(InputAction.CallbackContext context)
    {
        if (!isShiftPressed)
        {
            Debug.Log(string.Format("SwitchUp {0}", context));
        }
    }
    public void OnSwitchDown(InputAction.CallbackContext context)
    {
        if (!isShiftPressed)
        {
            Debug.Log(string.Format("SwitchDown {0}", context));
        }
    }

    public void OnSwitchLeft(InputAction.CallbackContext context)
    {
        Debug.Log(string.Format("SwitchLeft {0}", context));
    }

    public void OnSwitchRight(InputAction.CallbackContext context)
    {
        Debug.Log(string.Format("SwitchRight {0}", context));
    }

    #endregion

    private void Update()
    {
        // Debug.Log(moveInput.magnitude);
        // HandleMovement();

        // if (animateHandler.canRotate) {
        //     HandleRotation();
        // }
    }

    // private void HandleMovement()
    // {
    //     //Debug.Log(string.Format("forward {0}, right {1}",cameraTransform.forward, cameraTransform.right));
    //     Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
    //     Vector3 cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;

    //     moveDir = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;

    //     //Debug.Log(string.Format("cF: {0}, cR: {1}, mD: {2}", cameraForward, cameraRight, moveDir));

    //     if (moveDir.magnitude >= 0.1f)
    //     {
    //         characterController.Move(moveDir * moveSpeed * Time.deltaTime);
    //         animateHandler.UpdateAnimatorParameters(moveInput.magnitude, 0);
    //     }
    //     else
    //     {
    //         animateHandler.UpdateAnimatorParameters(0, 0);
    //     }
    // }

    // private void HandleRotation()
    // {
    //     if (moveDir.magnitude >= 0.1f)
    //     {
    //         Quaternion targetRotation = Quaternion.LookRotation(moveDir);
    //         transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    //     }
    // }
}
