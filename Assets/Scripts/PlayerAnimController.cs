using System.Collections;
using UnityEngine;

public static class AnimParams
{
    public static readonly int MoveState = Animator.StringToHash("MoveState");
    public static readonly int Trigger_Roll = Animator.StringToHash("Trigger_Roll");
    public static readonly int IsJumpBack = Animator.StringToHash("IsJumpBack");
    public static readonly int RootZTransition = Animator.StringToHash("RootZTransition");

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

    public void SetTrigger(int paramHash)
    {
        _animator.SetTrigger(paramHash);
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

        _activeTransitionCoroutine = StartCoroutine(SmoothTransitionCoroutine(paramHash, targetValue, dampTime));
    }

    private IEnumerator SmoothTransitionCoroutine(int paramHash, float targetValue, float dampTime)
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
}