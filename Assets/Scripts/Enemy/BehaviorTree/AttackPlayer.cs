using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AttackPlayer : Action
{
    [UnityEngine.Tooltip("EnemyAI")]
    public SharedEnemyAI enemyAI;

    private bool _isAttackFinished;

    public override void OnStart()
    {
        var ai = enemyAI.Value;
        if (ai == null)
        {
            _isAttackFinished = true;
            return;
        }

        _isAttackFinished = false;
        ai.AnimController.SetBool(EnemyAnimParams.IsAttacking, true);
        ai.AnimController.SetTrigger(EnemyAnimParams.Trigger_Attack);
    }

    public override TaskStatus OnUpdate()
    {
        var ai = enemyAI.Value;
        if (ai == null || _isAttackFinished)
        {
            return TaskStatus.Success;
        }

        var stateInfo = ai.AnimController.GetCurAnimatorStateInfo();

        if (stateInfo.shortNameHash == EnemyAnimParams.Attack && stateInfo.normalizedTime >= 1f)
        {
            ai.AnimController.SetBool(EnemyAnimParams.IsAttacking, false);
            _isAttackFinished = true;
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }


}
