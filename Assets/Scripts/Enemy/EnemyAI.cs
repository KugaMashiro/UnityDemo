using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Data")]
    public EnemyData data;

    [Header("External")]
    public GameObject player;

    [SerializeField] private EnemyAnimController _animController;
    public EnemyAnimController AnimController => _animController;

    // [SerializeField] private Rigidbody _controller;
    // public Rigidbody Controller=>_controller;

    [SerializeField] private CharacterController _controller;
    public CharacterController Controller;

    [HideInInspector] public bool shouldLookAt = true;
    //[HideInInspector]
    public bool isPerformRandomMove;
    [HideInInspector] public int randomSide; // right=1, left=-1
    [HideInInspector] public float moveDuration;
    [HideInInspector] public float curMoveTime;

    //[HideInInspector]
    public bool isChasing;
    private Vector3 _forwardDir;
    private Vector3 _rightDir;
    private Vector3 _smoothedForwardDir; // 平滑后的移动方向
    private float _dirSmoothSpeed = 0.2f;

    private Vector3 _prePos;

    private void Awake()
    {
        _animController = GetComponentInChildren<EnemyAnimController>();
        _controller = GetComponent<CharacterController>();
        //_controller = GetComponent<Rigidbody>();
        _prePos = transform.position;
    }
    public bool IsInRange(Transform target, float range)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position) <= range;
    }

    public void LookAtPlayer()
    {
        if (player == null) return;
        Vector3 targetPosition = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        transform.LookAt(targetPosition);
    }

    public void ResetMovement()
    {
        isPerformRandomMove = false;
        randomSide = 0;
        moveDuration = 0;
        curMoveTime = 0;
    }

    public void SetAnimWalkParams(float x, float z)
    {
        _animController.SetFloat(EnemyAnimParams.LockRelativeX, x);
        _animController.SetFloat(EnemyAnimParams.LockRelativeZ, z);
    }

    public void SetAnimWalkParamsSmooth(float x, float z, float dampTime = 0.1f)
    {
        _animController.SmoothTransition(EnemyAnimParams.LockRelativeX, x, EnemyAnimParams.LockRelativeZ, z, dampTime);
    }

    private void UpdateRelativeDirs()
    {
        _forwardDir = new Vector3(
            player.transform.position.x - transform.position.x,
            0,
            player.transform.position.z - transform.position.z
        ).normalized;

        _smoothedForwardDir = Vector3.Lerp(
            _smoothedForwardDir,
            _forwardDir,
            _dirSmoothSpeed
        ).normalized;

        _rightDir = Vector3.Cross(_smoothedForwardDir, Vector3.up).normalized;

    }

    private void Update()
    {
        
    }

    private void WanderRandomly()
    {
        UpdateRelativeDirs();

        Vector3 moveDir = _rightDir * randomSide;
        _controller.Move(moveDir * data.wonderSpeed * Time.fixedDeltaTime);
        //Vector3 movePos = transform.position + _rightDir * randomSide * data.wonderSpeed * Time.fixedDeltaTime;
        //_controller.MovePosition(movePos);

        curMoveTime += Time.fixedDeltaTime;
        if (curMoveTime >= moveDuration)
        {
            isPerformRandomMove = false;
        }
    }

    private void ChasePlayer()
    {
        UpdateRelativeDirs();
        //Vector3 movePos = transform.position + _smoothedForwardDir * data.chaseSpeed * Time.fixedDeltaTime;
        //Vector3 movePos = transform.position + Vector3.forward * data.chaseSpeed * Time.fixedDeltaTime;
        //Debug.Log($"{Vector3.Distance(transform.position, movePos) }");
        //_controller.MovePosition(movePos);
        _controller.Move(_smoothedForwardDir * data.chaseSpeed * Time.fixedDeltaTime);
    }

    private void FixedUpdate()
    {
        if (shouldLookAt)
        {
            LookAtPlayer();
        }
        if (isPerformRandomMove)
        {
            WanderRandomly();
        }

        if (isChasing)
        {
            ChasePlayer();
        }

        //Debug.Log($"cur speed: {Vector3.Distance(transform.position, _prePos) / Time.fixedDeltaTime}");
        _prePos = transform.position;
    }

}
