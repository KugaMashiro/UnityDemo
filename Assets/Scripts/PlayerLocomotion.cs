using UnityEngine;
using UnityEngine.UIElements;


public enum PlayerMotionType
{
    Idle = 0,
    Walk = 1,
    Run = 2,
}

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

    public void Move(Vector3 moveDir, float distance)
    {
        //Debug.Log(distance);

        if (MoveDirUtils.IsValidMoveDirection(moveDir))
        {
            _controller.Move(moveDir * distance);
        }
    }

    public void Face(Vector3 moveDir)
    {
        //Debug.Log($"in locomotion face, transform is {moveDir}, {MoveDirUtils.IsValidMoveDirection(moveDir)}");

        if (MoveDirUtils.IsValidMoveDirection(moveDir))
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _status.FaceRotateSpeed * Time.deltaTime);
        }
    }

    public void Face(Vector3 moveDir, float speed, float deltaTime)
    {
        //Debug.Log($"in locomotion face, transform is {moveDir}, {MoveDirUtils.IsValidMoveDirection(moveDir)}");

        if (MoveDirUtils.IsValidMoveDirection(moveDir))
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * deltaTime);
        }
    }
    public void ForceFace(Vector3 moveDir)
    {
        if (MoveDirUtils.IsValidMoveDirection(moveDir))
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = targetRotation;
        }
    }

    public void ForceFaceTarget(Transform targetTransform)
    {
        Vector3 towardsTarget = Vector3.Scale(targetTransform.position - transform.position, new Vector3(1, 0, 1));
        if (!MoveDirUtils.IsValidMoveDirection(towardsTarget))
        {
            towardsTarget = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
        }
        towardsTarget = towardsTarget.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(towardsTarget);
        transform.rotation = targetRotation;
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

    public Vector3 GetTargetRelativeMoveDirection(Vector2 moveInput, Transform targetTransform)
    {
        if (!MoveDirUtils.IsValidMoveDirection(moveInput))
            return Vector3.zero;

        Vector3 towardsTarget = Vector3.Scale(targetTransform.position - transform.position, new Vector3(1, 0, 1));
        if (!MoveDirUtils.IsValidMoveDirection(towardsTarget))
        {
            towardsTarget = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
        }
        towardsTarget = towardsTarget.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, towardsTarget).normalized;

        Vector3 moveDirection = (moveInput.y * towardsTarget + moveInput.x * right).normalized;

        return moveDirection;
    }

    public Vector2 GetInputFromMoveDirection(Vector3 moveDirection, Transform playerTransform, Transform targetTransform)
    {
        Vector3 dirXZ = Vector3.Scale(moveDirection, new Vector3(1, 0, 1)).normalized;
        if (!MoveDirUtils.IsValidMoveDirection(dirXZ))
            return Vector2.zero;

        Vector3 towardsTarget = Vector3.Scale(targetTransform.position - playerTransform.position, new Vector3(1, 0, 1));
        if (!MoveDirUtils.IsValidMoveDirection(towardsTarget))
        {
            towardsTarget = Vector3.Scale(playerTransform.forward, new Vector3(1, 0, 1));
        }
        Vector3 forward = towardsTarget.normalized;

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        float inputY = Vector3.Dot(dirXZ, forward);
        float inputX = Vector3.Dot(dirXZ, right);
        inputX = Mathf.Clamp(inputX, -1f, 1f);
        inputY = Mathf.Clamp(inputY, -1f, 1f);

        return new Vector2(inputX, inputY);
    }

    public Vector3 GetCurrentFacing()
    {
        return transform.forward;
    }
}