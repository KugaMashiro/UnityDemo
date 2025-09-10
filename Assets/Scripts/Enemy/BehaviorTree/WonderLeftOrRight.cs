using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class WonderLeftOrRight : Action
{
    [UnityEngine.Tooltip("EnemyAI")]
    public SharedEnemyAI enemyAI;

    private int _randomSide;

    public override void OnStart()
    {
        _randomSide = Random.Range(0, 2) == 0 ? 1 : -1;
        enemyAI.Value.randomSide = _randomSide;
        enemyAI.Value.isPerformRandomMove = true;
        enemyAI.Value.moveDuration = 2f;

        enemyAI.Value.curMoveTime = 0f;

        //enemyAI.Value.SetAnimWalkParams(-_randomSide * 0.5f, 0f);
        enemyAI.Value.SetAnimWalkParamsSmooth(-_randomSide * 0.5f, 0f);
    }

    public override TaskStatus OnUpdate()
    {
        if (!enemyAI.Value.isPerformRandomMove)
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        enemyAI.Value.isPerformRandomMove = false;
    }
}
