using UnityEngine;

public class HitState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;

    public HitState(PlayerStateManager manager)
    {
        _stateManager = manager;
    }


    public void Enter()
    {
        Debug.Log("Enter Hit");
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Hit);
        EventCenter.OnAnimAtkEnd += OnAnimAtkEnd;

    }
    public void Exit()
    {
        Debug.Log("Exit Hit");
        EventCenter.OnAnimAtkEnd -= OnAnimAtkEnd;
    }

    private void OnAnimAtkEnd()
    {
        Debug.Log("hit end");
        EventCenter.PublishStateChange(PlayerStateType.Idle);
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        
    }
}