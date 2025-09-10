using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class IsInRange : Conditional
{
    [UnityEngine.Tooltip("Enemy AI")]
    public SharedEnemyAI enemyAI;
    [UnityEngine.Tooltip("Judge Distance")]
    public float range;

    public override TaskStatus OnUpdate()
    {
        if (enemyAI.Value == null || enemyAI.Value.player == null)
        {
            return TaskStatus.Failure;
        }

        bool inRange = enemyAI.Value.IsInRange(
            enemyAI.Value.player.transform,
            range
        );

        return inRange ? TaskStatus.Success : TaskStatus.Failure;
    }

}
