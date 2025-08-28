using System.Collections;
using UnityEngine;

public static class AnimParams
{
    public static readonly int AnimStateIndex = Animator.StringToHash("AnimStateIndex");
    public static readonly int MoveState = Animator.StringToHash("MoveState");

    public static readonly int LockRelativeX = Animator.StringToHash("LockRelativeX");
    public static readonly int LockRelativeZ = Animator.StringToHash("LockRelativeZ");
    public static readonly int MotionType = Animator.StringToHash("MotionType");
    public static readonly int Trigger_Roll = Animator.StringToHash("Trigger_Roll");
    public static readonly int IsJumpBack = Animator.StringToHash("IsJumpBack");
    public static readonly int RootZTransitionL0 = Animator.StringToHash("RootZTransitionL0");
    public static readonly int RootZTransitionL1 = Animator.StringToHash("RootZTransitionL1");
    public static readonly int Trigger_Atk = Animator.StringToHash("Trigger_Atk");

    public static readonly int AtkComboIndex = Animator.StringToHash("AtkComboIndex");
    public static readonly int AtkChargable = Animator.StringToHash("AtkChargable");
    public static readonly int AtkType = Animator.StringToHash("AtkType");

    public static readonly int Trigger_AtkExit = Animator.StringToHash("Trigger_AtkExit");
    public static readonly int Trigger_ChargeExit = Animator.StringToHash("Trigger_ChargeExit");
    public static readonly int Trigger_Hit = Animator.StringToHash("Trigger_Hit");
}

public static class AnimStates
{
    public static readonly int Roll = Animator.StringToHash("Roll_8Dir");
    public static readonly int JumpBack = Animator.StringToHash("Jump_B");
}
public class PlayerAnimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;

    public int MoveStateHash { get; private set; }

    private Coroutine _activeTransitionCoroutine;

    public Animator Animator => _animator;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    public void ResetCoroutine()
    {
        if (_activeTransitionCoroutine != null)
        {
            StopCoroutine(_activeTransitionCoroutine);
            _activeTransitionCoroutine = null;
        }
    }

    public void SetBool(int paramHash, bool value)
    {
        _animator.SetBool(paramHash, value);
    }

    public void SetInteger(int paramHash, int value)
    {
        _animator.SetInteger(paramHash, value);
    }

    public void SetFloat(int paramHash, float value)
    {
        _animator.SetFloat(paramHash, value);
    }

    public void SetMotionType(PlayerMotionType type)
    {
        _animator.SetInteger(AnimParams.MotionType, (int)type);
        // switch (type)
        // {
        //     case PlayerMotionType.Idle:
        //         _animator.SetInteger(AnimParams.MotionType, (int)type);
        //         break;
        //     case PlayerMotionType.Walk:
        //         _animator.SetInteger(AnimParams.MotionType, 1);

        // }
    }

    public void SetAtkType(AttackType type)
    {
        _animator.SetInteger(AnimParams.AtkType, (int)type);
    }

    public void SetAnimStateIndex(AnimStateIndex index)
    {
        _animator.SetInteger(AnimParams.AnimStateIndex, (int)index);
    }

    public void SetTrigger(int paramHash)
    {
        _animator.SetTrigger(paramHash);
    }

    public void ResetTrigger(int paramHash)
    {
        _animator.ResetTrigger(paramHash);
    }

    public float GetFloat(int paramHash)
    {
        return _animator.GetFloat(paramHash);
    }

    public bool IsInTransition(int layerIndex)
    {
        return _animator.IsInTransition(layerIndex);
    }

    public void SmoothTransition(int paramHash, float targetValue, float dampTime)
    {
        if (_activeTransitionCoroutine != null)
            StopCoroutine(_activeTransitionCoroutine);

        _activeTransitionCoroutine = StartCoroutine(SmoothTransitionSingle(paramHash, targetValue, dampTime));
    }

    private IEnumerator SmoothTransitionSingle(int paramHash, float targetValue, float dampTime)
    {
        const float threshold = 0.01f;

        while (true)
        {
            _animator.SetFloat(paramHash, targetValue, dampTime, Time.deltaTime);

            float currentValue = _animator.GetFloat(paramHash);
            if (Mathf.Abs(currentValue - targetValue) < threshold)
            {
                _animator.SetFloat(paramHash, targetValue);
                break;
            }

            yield return null;
        }

        _activeTransitionCoroutine = null;
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
}