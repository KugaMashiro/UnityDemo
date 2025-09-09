using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyAnimParams
{
    public static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    public static readonly int Trigger_Attack = Animator.StringToHash("Trigger_Attack");

    public static readonly int Attack = Animator.StringToHash("Attack");
}

public class EnemyAnimController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetBool(int paramHash, bool value)
    {
        _animator.SetBool(paramHash, value);
    }

    public void SetTrigger(int paramHash)
    {
        _animator.SetTrigger(paramHash);
    }

    public AnimatorStateInfo GetCurAnimatorStateInfo()
    {
        return _animator.GetCurrentAnimatorStateInfo(0);
    }
}
