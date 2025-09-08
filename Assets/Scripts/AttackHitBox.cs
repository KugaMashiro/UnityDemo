using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    [SerializeField] private CapsuleCollider _weaponCollider;

    [Header("Performance Settings")]
    [SerializeField] private int _maxColliders = 10;

    private Collider[] _hitColliders;
    public Collider[] HitColliders => _hitColliders;
    private HashSet<GameObject> _hitTargets = new HashSet<GameObject>();

    private List<CapsuleData> _debugHitboxes = new List<CapsuleData>();
    private float _debugHitboxLifetime = 2f;

    private void Awake()
    {
        _weaponCollider = GetComponent<CapsuleCollider>();
        _hitColliders = new Collider[_maxColliders];
    }

    struct CapsuleData
    {
        public Vector3 point1;
        public Vector3 point2;
        public float radius;
        public float timestamp;

        public CapsuleData(Vector3 _point1, Vector3 _point2, float _radius, float _timestamp)
        {
            point1 = _point1;
            point2 = _point2;
            radius = _radius;
            timestamp = _timestamp;
        }
    }

    private CapsuleData GetCurCapsuleCollider()
    {
        Vector3 worldCenter = transform.TransformPoint(_weaponCollider.center);
        Vector3 halfExtent = transform.up * (_weaponCollider.height / 2 - _weaponCollider.radius);
        Vector3 point1 = worldCenter + halfExtent;
        Vector3 point2 = worldCenter - halfExtent;

        return new CapsuleData(point1, point2, _weaponCollider.radius, Time.time);
    }

    public int DetectHits(int layerMask)
    {
        // Vector3 worldCenter = transform.TransformPoint(_weaponCollider.center);
        // Vector3 halfExtent = transform.up * (_weaponCollider.height / 2 - _weaponCollider.radius);
        // Vector3 point1 = worldCenter + halfExtent;
        // Vector3 point2 = worldCenter - halfExtent;
        CapsuleData capsule = GetCurCapsuleCollider();

        _debugHitboxes.Add(capsule);

        return Physics.OverlapCapsuleNonAlloc(capsule.point1, capsule.point2, capsule.radius, _hitColliders, 1 << layerMask);
        //return Physics.OverlapCapsuleNonAlloc(point1, point2, _weaponCollider.radius, _hitColliders, 1 << layerMask);
    }

    private void FixedUpdate()
    {
        // int numHits = DetectHits(LayerMasksIntger.EnemyDamage);
        // if (numHits != 0)
        // {
        //     Debug.Log("Hit!");
        // }
        _debugHitboxes.RemoveAll(data => Time.time - data.timestamp > _debugHitboxLifetime);
    }

    private void DrawCapsule(in CapsuleData capsule, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(capsule.point1, capsule.radius);
        Gizmos.DrawWireSphere(capsule.point2, capsule.radius);

        Gizmos.DrawLine(capsule.point1 + transform.right * capsule.radius, capsule.point2 + transform.right * capsule.radius);
        Gizmos.DrawLine(capsule.point1 - transform.right * capsule.radius, capsule.point2 - transform.right * capsule.radius);
        Gizmos.DrawLine(capsule.point1 + transform.forward * capsule.radius, capsule.point2 + transform.forward * capsule.radius);
        Gizmos.DrawLine(capsule.point1 - transform.forward * capsule.radius, capsule.point2 - transform.forward * capsule.radius);
    }

    private void OnDrawGizmosSelected()
    {
        // Vector3 worldCenter = transform.TransformPoint(_weaponCollider.center);
        // Vector3 halfExtent = transform.up * (_weaponCollider.height / 2 - _weaponCollider.radius);
        // Vector3 point1 = worldCenter + halfExtent;
        // Vector3 point2 = worldCenter - halfExtent;

        // //CapsuleData capsule = GetCurCapsuleCollider();

        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireSphere(point1, _weaponCollider.radius);
        // Gizmos.DrawWireSphere(point2, _weaponCollider.radius);

        // Gizmos.DrawLine(point1 + transform.right * _weaponCollider.radius, point2 + transform.right * _weaponCollider.radius);
        // Gizmos.DrawLine(point1 - transform.right * _weaponCollider.radius, point2 - transform.right * _weaponCollider.radius);
        // Gizmos.DrawLine(point1 + transform.forward * _weaponCollider.radius, point2 + transform.forward * _weaponCollider.radius);
        // Gizmos.DrawLine(point1 - transform.forward * _weaponCollider.radius, point2 - transform.forward * _weaponCollider.radius);

        CapsuleData capsule = GetCurCapsuleCollider();
        DrawCapsule(capsule, Color.blue);
        
        foreach (var data in _debugHitboxes)
        {
            float alpha = 1 - (Time.time - data.timestamp) / _debugHitboxLifetime;
            // 绘制胶囊体
            DrawCapsule(data, new Color(0, 1, 0, alpha));
        }
        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireSphere(capsule.point1, capsule.radius);
        // Gizmos.DrawWireSphere(capsule.point2, capsule.radius);

        // Gizmos.DrawLine(capsule.point1 + transform.right * capsule.radius, capsule.point2 + transform.right * capsule.radius);
        // Gizmos.DrawLine(capsule.point1 - transform.right * capsule.radius, capsule.point2 - transform.right * capsule.radius);
        // Gizmos.DrawLine(capsule.point1 + transform.forward * capsule.radius, capsule.point2 + transform.forward * capsule.radius);
        // Gizmos.DrawLine(capsule.point1 - transform.forward * capsule.radius, capsule.point2 - transform.forward * capsule.radius);
    }


}
