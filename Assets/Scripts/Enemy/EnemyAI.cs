using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
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
    public CharacterController Controller => _controller;

    [Header("Combo Data")]
    public List<EnemyComboData> ComboData;
    private Dictionary<ComboCode, float> _comboCoolDownEndTime = new Dictionary<ComboCode, float>();
    public float rigidityEndTime;
    public bool isAttacking;

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

    private bool _needHitDetection;

    public AttackHitBox CurEnemyWeaponHitBox;

    //private readonly System.Action _onAnimAtkCheck = OnAnimAtkCheck;

    private void Awake()
    {
        _animController = GetComponentInChildren<EnemyAnimController>();
        _controller = GetComponent<CharacterController>();
        //_controller = GetComponent<Rigidbody>();
        _prePos = transform.position;

        InitComboCoolDownDict();
        CurEnemyWeaponHitBox = GetComponentInChildren<AttackHitBox>();
    }

    private void OnEnable()
    {
        EventCenter.OnEnemyAnimAtkCheck += OnEnemyAnimAtkCheck;
    }

    private void OnDisable()
    {
        EventCenter.OnEnemyAnimAtkCheck -= OnEnemyAnimAtkCheck; 
    }

    private void OnEnemyAnimAtkCheck()
    {
        _needHitDetection = true;
        //Debug.Log("Enemy Attack!");
    }

    private void InitComboCoolDownDict()
    {
        foreach (var data in ComboData)
        {
            if (!_comboCoolDownEndTime.ContainsKey(data.comboCode))
            {
                _comboCoolDownEndTime[data.comboCode] = 0;
            }
        }
    }

    public void SetComboCoolDown(ComboCode combo, float coolDownTime)
    {
        _comboCoolDownEndTime[combo] = coolDownTime;
    }

    public EnemyComboData GetValidComboByWeight()
    {
        List<EnemyComboData> availableCombos = new List<EnemyComboData>();
        foreach (var combo in ComboData)
        {
            if (Time.time >= _comboCoolDownEndTime[combo.comboCode])
            {
                availableCombos.Add(combo);
            }
        }

        //Debug.Log(availableCombos.Count);
        if (availableCombos.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;
        foreach (var combo in availableCombos)
        {
            totalWeight += combo.comboWeight;
        }

        int randomValue = Random.Range(0, totalWeight);

        int currentWeight = 0;
        foreach (var combo in availableCombos)
        {
            currentWeight += combo.comboWeight;
            if (randomValue < currentWeight)
            {
                return combo;
            }
        }
        return null;
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

        //Vector3 moveDir = _rightDir * randomSide;

        //_controller.Move(moveDir * data.wonderSpeed * Time.fixedDeltaTime);


        Vector3 movePos = transform.position + _rightDir * randomSide * data.wonderSpeed * Time.fixedDeltaTime;
        transform.position = movePos;
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

        Vector3 movePos = transform.position + _smoothedForwardDir * data.chaseSpeed * Time.fixedDeltaTime;
        //Vector3 movePos = transform.position + Vector3.forward * data.chaseSpeed * Time.fixedDeltaTime;
        //Debug.Log($"{Vector3.Distance(transform.position, movePos) }");
        //_controller.MovePosition(movePos);

        //_controller.Move(_smoothedForwardDir * data.chaseSpeed * Time.fixedDeltaTime);
        transform.position = movePos;
    }

    private void AtkCheck()
    {
        int hitnums = CurEnemyWeaponHitBox.DetectHits(LayerMasksIntger.PlayerDamage);
        if (hitnums != 0)
        {
            Debug.Log(" Enemy Hit Detected! ");
        }
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

        if (_needHitDetection)
        {
            AtkCheck();
            _needHitDetection = false;
        }

        //Debug.Log($"cur speed: {Vector3.Distance(transform.position, _prePos) / Time.fixedDeltaTime}");
        //_prePos = transform.position;
    }

}
