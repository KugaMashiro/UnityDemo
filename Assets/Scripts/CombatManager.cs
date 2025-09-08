using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerMasksIntger
{
    public static readonly int PlayerDamage = LayerMask.NameToLayer("PlayerDamage");
    public static readonly int EnemyDamage = LayerMask.NameToLayer("EnemyDamage");
    public static readonly int PlayerAttack = LayerMask.NameToLayer("PlayerAttack");
    public static readonly int EnemyAttack = LayerMask.NameToLayer("EnemyAttack");
}

//collider.transform.parent.gameObject

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // public bool CheckPlayerAtkHit(CapsuleCollider collider)
    // {

    // }

}
