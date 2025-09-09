using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Data")]
    public EnemyData data;

    [Header("External")]
    public GameObject player;

    [SerializeField] private EnemyAnimController _animController;
    public EnemyAnimController AnimController => _animController;

    private void Awake()
    {
        _animController = GetComponentInChildren<EnemyAnimController>();
    }
    public bool IsInRange(Transform target, float range)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position) <= range;
    }
}
