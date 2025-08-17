using System;
using UnityEngine;

public enum PlayerStateType
{
    Idle,
    Walk,
    Run,
    Roll
}

public class MovementInputEventArgs : EventArgs, IPoolable
{
    private Vector2 _movement;
    //private bool _hasMovement;

    public Vector2 Movement => _movement;
    // {
    //     get => _movement;
    //     set
    //     {
    //         _movement = value;
    //         HasMovement = _movement.sqrMagnitude > 0.01f;
    //     }
    // }
    public void SetMovement(in Vector2 movement)
    {
        _movement = movement;
        HasMovement = MoveDirUtils.IsValidMoveDirection(_movement);
    }
    public bool HasMovement { get; private set; }
    public bool IsInUse { get; set; }

    public void Reset()
    {
        _movement = Vector2.zero;
        HasMovement = false;
        //SetMovement(Vector2.zero);
        //IsInUse = false;
    }

    // public void Initialize(in Vector2 movement)
    // {
    //     Movement = movement;
    //     //IsInUse = true;
    // }
}

public class StateChangeEventArgs : EventArgs, IPoolable
{
    public PlayerStateType TargetState { get; set; }
    public bool IsInUse { get; set; }
    public void Reset()
    {
        TargetState = PlayerStateType.Idle;
    }
}

public static class EventCenter
{

    public static event Action<StateChangeEventArgs> OnStateChange;
    public static event Action<MovementInputEventArgs> OnMovementInput;

    public static event Action OnRunButtunPressed;

    public static event Action OnRollButtonPressed;

    public static event Action OnAnimRollEnd;


    public static void PublishMovementInput(Vector2 movementInput)
    {

        var args = EventPoolManager.Instance.GetPool<MovementInputEventArgs>().Get();
        args.SetMovement(movementInput);

        OnMovementInput?.Invoke(args);

        EventPoolManager.Instance.GetPool<MovementInputEventArgs>().Release(args);
    }

    public static void PublishStateChange(PlayerStateType targetState)
    {
        var args = EventPoolManager.Instance.GetPool<StateChangeEventArgs>().Get();
        args.TargetState = targetState;

        OnStateChange?.Invoke(args);

        EventPoolManager.Instance.GetPool<StateChangeEventArgs>().Release(args);
    }

    public static void PublishRunButtonPressed()
    {
        OnRunButtunPressed?.Invoke();
    }

    public static void PublishRollButtonPressed()
    {
        OnRollButtonPressed?.Invoke();
    }

    public static void PublicAnimRollEnd()
    {
        OnAnimRollEnd?.Invoke();
    }
}
