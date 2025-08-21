using System;
using Unity.VisualScripting;
using UnityEngine;

// public enum PlayerStateType
// {
//     Idle,
//     Walk,
//     Run,
//     Roll,
//     Attack
// }

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

public class BufferedInputEventArgs : EventArgs, IPoolable
{
    public uint InputUniqueId { get; set; }
    public bool IsInUse { get; set; }

    public void Reset()
    {
        InputUniqueId = 0;
        IsInUse = false;
    }
}

public static class EventCenter
{

    public static event Action<StateChangeEventArgs> OnStateChange;
    public static event Action<MovementInputEventArgs> OnMovementInput;

    public static event Action OnRunButtunPressed;

    public static event Action<BufferedInputEventArgs> OnRollButtonPressed;
    public static event Action<BufferedInputEventArgs> OnAttackMainPerformed;

    public static event Action OnAnimRollEnd;
    public static event Action OnAnimAtkEnd;

    public static event Action OnAnimInteractWindowOpen;


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

    public static void PublishRollButtonPressed(uint UniqueId)
    {
        var args = EventPoolManager.Instance.GetPool<BufferedInputEventArgs>().Get();
        args.InputUniqueId = UniqueId;

        //Debug.Log($"EventCenter: {args.InputUniqueId}");
        OnRollButtonPressed?.Invoke(args);

        EventPoolManager.Instance.GetPool<BufferedInputEventArgs>().Release(args);
    }

    public static void PublishAtkMainPerformed(uint UniqueId)
    {
        var args = EventPoolManager.Instance.GetPool<BufferedInputEventArgs>().Get();
        args.InputUniqueId = UniqueId;

        OnAttackMainPerformed?.Invoke(args);

        EventPoolManager.Instance.GetPool<BufferedInputEventArgs>().Release(args);
    }
    public static void PublishAnimRollEnd()
    {
        OnAnimRollEnd?.Invoke();
    }

    public static void PublishAnimAtkEnd()
    {
        OnAnimAtkEnd?.Invoke();
    }

    public static void PublishAnimInteractWindowOpen()
    {
        OnAnimInteractWindowOpen?.Invoke();
    }
}
