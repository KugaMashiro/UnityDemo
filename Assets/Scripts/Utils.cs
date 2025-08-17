
using UnityEngine;

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
}