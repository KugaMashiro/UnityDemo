using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AttackCombo : Action
{
    [UnityEngine.Tooltip("EnemyAI")]
    public SharedEnemyAI enemyAI;

    private bool _isAttackFinished;
    private bool _isAttacking;
    private EnemyComboData _data;

    public override void OnStart()
    {
        var ai = enemyAI.Value;

        _isAttackFinished = false;
        _isAttacking = false;
    }

    public override TaskStatus OnUpdate()
    {
        var ai = enemyAI.Value;

        if (ai == null || _isAttackFinished)
        {
            return TaskStatus.Success;
        }

        if (Time.time < ai.rigidityEndTime)
        {
            return TaskStatus.Failure;
        }

        if (_isAttacking)
        {
            var stateInfo = ai.AnimController.GetCurAnimatorStateInfo();

            if (stateInfo.shortNameHash == _data.ComboShortHash && stateInfo.normalizedTime >= 1f)
            {
                ai.AnimController.SetBool(EnemyAnimParams.IsAttacking, false);
                _isAttackFinished = true;
                ai.rigidityEndTime = Time.time + _data.comboRigidity;
                return TaskStatus.Success;
            }
        }

        else
        {
            _data = ai.GetValidComboByWeight();
            if (_data == null)
            {
                Debug.Log("No valid combo!");
                return TaskStatus.Failure;
            }
            else
            {
                _isAttacking = true;
                ai.SetComboCoolDown(_data.comboCode, Time.time + _data.comboCoolDown);
                ai.AnimController.SetBool(EnemyAnimParams.IsAttacking, true);
                ai.AnimController.SetTrigger(EnemyAnimParams.Trigger_Attack);
                ai.AnimController.SetInteger(EnemyAnimParams.ComboCode, (int)_data.comboCode);
                enemyAI.Value.SetAnimWalkParamsSmooth(0f, 0f);

            }
        }

        return TaskStatus.Running;
    }



}