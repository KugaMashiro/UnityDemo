using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Componenet ref")]
    private CharacterController _controller;
    private PlayerStatus _status;

    public CharacterController Controller => _controller;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _status = GetComponent<PlayerStatus>();
    }

    public void Move(Vector3 moveDir, float speed, float deltaTime)
    {
        if (MoveDirUtils.IsValidMoveDirection(moveDir))
        {
            _controller.Move(moveDir * speed * deltaTime);
        }
    }

    public void Face(Vector3 moveDir)
    {
        //Debug.Log($"in locomotion face, transform is {transform}");

        if (MoveDirUtils.IsValidMoveDirection(moveDir))
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _status.faceRotateSpeed * Time.deltaTime);
        }
    }

    public Vector3 GetCameraRelativeMoveDirection(Vector2 moveInput, Transform cameraTransform)
    {
        if (!MoveDirUtils.IsValidMoveDirection(moveInput))
            return Vector3.zero;
        
        Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;

        Vector3 moveDirection = (moveInput.y * cameraForward + moveInput.x * cameraRight).normalized;

        return moveDirection;
    }
}