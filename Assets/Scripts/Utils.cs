
using UnityEngine;

public static class GlobalConstants
{
    public const float ROOTTZ_EPSILON = 0.2f;
    public const float FLOAT_EPSILON = 0.001f;
}

public static class MoveDirUtils
{
    private const float _moveThresholdSquared = 0.01f;

    public static bool IsValidMoveDirection(in Vector2 moveDir)
    {
        return moveDir.sqrMagnitude > _moveThresholdSquared;
    }

    public static bool IsValidMoveDirection(in Vector3 moveDir)
    {
        return moveDir.sqrMagnitude > _moveThresholdSquared;
    }

    // public static bool IsValidMoveDirection(in Vector3? moveDir)
    // {
    //     return moveDir.HasValue && IsValidMoveDirection(moveDir.Value);
    // }
}

public static class FloatUtils
{
    public static bool FloatEqual(float a, float b)
    {
        return Mathf.Abs(a - b) < GlobalConstants.FLOAT_EPSILON;
    }
}