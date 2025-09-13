using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyAnimParams
{
    public static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    public static readonly int Trigger_Attack = Animator.StringToHash("Trigger_Attack");
    public static readonly int LockRelativeX = Animator.StringToHash("LockRelativeX");
    public static readonly int LockRelativeZ = Animator.StringToHash("LockRelativeZ");
    public static readonly int ComboCode = Animator.StringToHash("Combocode");

    public static readonly int Attack = Animator.StringToHash("Attack");
}

public class EnemyAnimController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;

    private Coroutine _activeTransitionCoroutine;

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

    public void SetInteger(int paramHash, int value)
    {
        _animator.SetInteger(paramHash, value);
    }


    public void SetFloat(int paramHash, float value)
    {
        _animator.SetFloat(paramHash, value);
    }

    public AnimatorStateInfo GetCurAnimatorStateInfo()
    {
        return _animator.GetCurrentAnimatorStateInfo(0);
    }

    public void SmoothTransition(int param1, float value1, int param2, float value2, float dampTime)
    {
        if (_activeTransitionCoroutine != null)
            StopCoroutine(_activeTransitionCoroutine);

        _activeTransitionCoroutine = StartCoroutine(SmoothTransitionDual(param1, value1, param2, value2, dampTime));
    }

    private IEnumerator SmoothTransitionDual(int param1, float value1, int param2, float value2, float dampTime)
    {
        const float threshold = 0.01f;
        bool isFirstComplete = false;
        bool isSecondComplete = false;

        while (!(isFirstComplete && isSecondComplete))
        {
            if (!isFirstComplete)
            {
                _animator.SetFloat(param1, value1, dampTime, Time.deltaTime);
                float current1 = _animator.GetFloat(param1);
                isFirstComplete = Mathf.Abs(current1 - value1) < threshold;
                if (isFirstComplete)
                    _animator.SetFloat(param1, value1);
            }

            if (!isSecondComplete)
            {
                _animator.SetFloat(param2, value2, dampTime, Time.deltaTime);
                float current2 = _animator.GetFloat(param2);
                isSecondComplete = Mathf.Abs(current2 - value2) < threshold;
                if (isSecondComplete)
                    _animator.SetFloat(param2, value2);
            }

            yield return null;
        }

        _activeTransitionCoroutine = null;
    }

    public void OnEnemyAnimAtkCheck()
    {
        EventCenter.PublishEnemyAnimAtkCheck();
    }
}
