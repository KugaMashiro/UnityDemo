using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class CanAttack : Conditional
{
    [UnityEngine.Tooltip("Enemy AI")]
    public SharedEnemyAI enemyAI;

    public override TaskStatus OnUpdate()
    {
        if (enemyAI.Value == null || enemyAI.Value.player == null)
        {
            return TaskStatus.Failure;
        }

        bool inRange = enemyAI.Value.IsInRange(
            enemyAI.Value.player.transform,
            enemyAI.Value.data.atkRange
        );

        return inRange ? TaskStatus.Success : TaskStatus.Failure;
    }

}
