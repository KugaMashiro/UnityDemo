using BehaviorDesigner.Runtime;
using UnityEngine;

[System.Serializable]
public class SharedEnemyAI : SharedVariable<EnemyAI>
{
    public static implicit operator SharedEnemyAI(EnemyAI value)
    {
        return new SharedEnemyAI { Value = value };
    }
}