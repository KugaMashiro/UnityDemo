using UnityEngine;

public class DeadState : IPlayerState
{
    private readonly PlayerStateManager _stateManager;
    private AnimatorStateInfo _stateInfo;

    public DeadState(PlayerStateManager manager)
    {
        _stateManager = manager;
    }

    public void Enter()
    {
        Debug.Log("Enter Dead");
        _stateManager.AnimController.ResetTrigger(AnimParams.Trigger_Revive);
        _stateManager.AnimController.SetTrigger(AnimParams.Trigger_Dead);
    }

    public void Exit()
    {

    }

    public void FixedUpdate()
    {

    }

    public void LateUpdate()
    {
        _stateInfo = _stateManager.AnimBaseLayerInfo();
        if (_stateInfo.shortNameHash == AnimStates.Dead)
        {
            if (_stateInfo.normalizedTime >= 0.99f)
            {
                Debug.Log("DeadAnim End");
                //_stateManager.AnimController.ResetTrigger(Anim)
                CheckPointSystem.Instance.Revive();
            }
        }
    }

    public void Update()
    {

    }
}
