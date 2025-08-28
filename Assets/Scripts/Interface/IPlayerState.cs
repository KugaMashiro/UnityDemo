using UnityEngine;

public interface IPlayerState
{
    void Enter();

    void Update();

    void FixedUpdate();

    void Exit();

    void LateUpdate();

    // void HandleMovement();

    // void HandleRoll();

    // void HandleAttack();

    // void HandleHit();   
}
