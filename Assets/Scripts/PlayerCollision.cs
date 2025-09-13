using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private CharacterController _cc;
    private Vector3 microMove = Vector3.zero;
    private float MinMoveThreshold;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        MinMoveThreshold = 0.1f;
    }

    void FixedUpdate()
    {
        microMove = Vector3.up * MinMoveThreshold;

        CollisionFlags flags = _cc.Move(microMove * Time.fixedDeltaTime);
        if ((flags & CollisionFlags.Above) == 0)
        {
            _cc.Move(-microMove * Time.fixedDeltaTime);
        }
    }
}
