using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ChasePlayer : Action
{
    [UnityEngine.Tooltip("EnemyAI")]
    public SharedEnemyAI enemyAI;

    [UnityEngine.Tooltip("Near Distance")]
    public float range = 5f;

    public override void OnStart()
    {
        enemyAI.Value.isChasing = true;
        enemyAI.Value.SetAnimWalkParamsSmooth(0f, 2f);

    }

    public override TaskStatus OnUpdate()
    {
        bool isinrange = enemyAI.Value.IsInRange(enemyAI.Value.player.transform, range);
        //Debug.Log($"Is in range: {isinrange}");
        if (isinrange)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        enemyAI.Value.isChasing = false;
        enemyAI.Value.SetAnimWalkParamsSmooth(0f, 0f);
    }
}
