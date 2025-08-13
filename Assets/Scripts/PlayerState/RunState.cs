using UnityEngine;

public class RunState: IPlayerState 
{
    private PlayerStateManager stateManger;
    private Vector2 movementInput;

    public RunState(PlayerStateManager manager) 
    {
        stateManger = manager;
    }

    public void Enter()
    {

    }

    public void Update() 
    {

    }

    public void Exit() 
    {

    }

    public void HandleRoll() 
    {

    }

    public void HandleAttack()
    {

    }

    public void HandleHit() 
    {

    }

    public void HandleMovement()
    {
        
    }

    public void FixedUpdate()
    {
        //throw new System.NotImplementedException();
    }
}