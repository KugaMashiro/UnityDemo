
using UnityEditor.Animations;
using UnityEngine;

public class ReviveState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;

    private AnimatorStateInfo _stateInfo;

    public ReviveState(PlayerStateManager manager)
    {
        _stateManager = manager;
    }

    public void Enter()
    {
        _stateManager.Status.SetStaminaDeltaPerSecond(0f);
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_Revive);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Revive);
    }

    public void Exit()
    {
        
    }

    public void FixedUpdate()
    {
        
    }

    private void TransToIdle()
    {
        EventCenter.PublishStateChange(PlayerStateType.Idle);
    }
    
    public void LateUpdate()
    {
        _stateInfo = _stateManager.AnimBaseLayerInfo();
        if (_stateInfo.shortNameHash == AnimStates.Revive)
        {
            if (_stateInfo.normalizedTime >= 0.99f)
            {
                _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_ReviveExit);
                _stateManager.AnimController.SetTrigger(AnimParams.Trigger_ReviveExit);
                TransToIdle();
            }
        }
    }

    public void Update()
    {
        
    }
}
