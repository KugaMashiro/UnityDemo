using Cinemachine;
using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    [SerializeField] private GameObject _lockedTarget;
    public GameObject LockedTarget => _lockedTarget;
    private bool _isLocked;
    public bool IsLocked => _isLocked;

    //Transform targetTransform => _lockedTarget.transform;

    [SerializeField] private CinemachineVirtualCamera _freeCamera;
    [SerializeField] private CinemachineVirtualCamera _lockOnCamera;

    private CinemachineBrain _cinemachineBrain;

    private void Awake()
    {
        _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        if (_lockOnCamera != null)
        {
            _lockOnCamera.Priority = 0;
            _lockOnCamera.LookAt = null;
        }
        else
        {
            Debug.LogError("LockOnCamera Not Set!");
        }
    }

    private bool TryGetTarget()
    {
        return true;
    }

    public bool TryLock()
    {
        if (!TryGetTarget()) return false;

        _lockOnCamera.LookAt = _lockedTarget.transform;
        _lockOnCamera.Priority = 20;

        _cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(
            CinemachineBlendDefinition.Style.HardIn,
            0.5f
        );

        _isLocked = true;
        return true;
    }

    public void UnLock()
    {
        if (!_isLocked) return;
        _lockOnCamera.LookAt = null;
        _lockOnCamera.Priority = 0;
        SyncFreeCameraToLockOnCamera();
        _cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(
            CinemachineBlendDefinition.Style.EaseInOut,
            0.5f
        );
        _isLocked = false;
    }

    private void SyncFreeCameraToLockOnCamera()
    {
        _freeCamera.transform.position = _lockOnCamera.transform.position;
        _freeCamera.transform.rotation = _lockOnCamera.transform.rotation;

        CinemachinePOV freePOV = _freeCamera.GetCinemachineComponent<CinemachinePOV>();
        if (freePOV != null)
        {
            Vector3 eulerRotation = _lockOnCamera.transform.rotation.eulerAngles;
            freePOV.m_HorizontalAxis.Value = eulerRotation.y;
            freePOV.m_VerticalAxis.Value = eulerRotation.x;
        }
    }
}
