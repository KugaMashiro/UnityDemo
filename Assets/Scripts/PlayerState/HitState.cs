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
        EventCenter.OnAnimAtkEnd += OnAnimAtkEnd;
        _stateManager.Status.SetStaminaDeltaPerSecond(_stateManager.Status.NormalStaminaDelta);

        Debug.Log("Enter Hit");
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Hit);

    }
    public void Exit()
    {
        Debug.Log("Exit Hit");
        EventCenter.OnAnimAtkEnd -= OnAnimAtkEnd;
    }

    private void OnAnimAtkEnd(int i)
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
    
    public void LateUpdate()
    {
        
    }

}