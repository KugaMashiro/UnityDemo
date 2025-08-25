using System.Collections.Generic;
using UnityEditor.Animations;

using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Player/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Name")]
    public string weaponName;

    [Header("Weapon Animation")]
    public AnimatorController attackLayerController;

    [Header("Attack Parameters")]
    public List<AttackParams> attackParamsList = new();

    private Dictionary<AttackType, AttackParams> _atkParamsMap;

    private void InitMap()
    {
        if (_atkParamsMap == null)
        {
            _atkParamsMap = new Dictionary<AttackType, AttackParams>();
            foreach (var param in attackParamsList)
            {
                if (_atkParamsMap.ContainsKey(param.attackType))
                {
                    Debug.LogError($"Attack Type {param.attackType} has already exists in weapon {weaponName}!");
                    continue;
                }
                _atkParamsMap[param.attackType] = param;
            }
        }
    }

    public AnimatorControllerLayer GetAnimatorControllerLayer()
    {
        if (attackLayerController != null && attackLayerController.layers.Length > 0)
        {
            return attackLayerController.layers[0];
        }
        Debug.LogError($"Weapon {weaponName} doesn't have animation layer!");
        return null;
    }

    public AttackParams GetAtkParams(AttackType type)
    {
        InitMap();
        if (_atkParamsMap.TryGetValue(type, out var param))
            return param;

        Debug.LogError($"Weapon {weaponName} doesn't have params of AttackType {type}!");
        return null;
    }

    public float GetMoveDistance(AttackType type, int comboStage)
    {
        var param = GetAtkParams(type);
        if (param == null || comboStage < 0 || comboStage >= param.moveDistances.Count)
        {
            Debug.LogError($"Weapon {weaponName} doesn't have {type} stage {comboStage} distance!");
            return 0;
        }
        return param.moveDistances[comboStage];
    }

    // public int GetMaxComboCnt(AttackType type, int comboStage)
    // {
    //     var param = GetAtkParams(type);
    //     if (param == null || comboStage < 0 || comboStage >= param.moveDistances.Count)
    //     {
    //         Debug.LogError($"Weapon {weaponName} doesn't have {type} maxComboC!");
    //         return 0;
    //     }
    //     return param.moveDistances[comboStage];
    // }

    public int GetMaxComboCnt(AttackType type)
    {
        var param = GetAtkParams(type);
        if (param == null)
        {
            Debug.LogError($"Weapon {weaponName} doesn't have {type} maxComboCnt!");
            return 1;
        }
        return param.maxComboCnt;
    }

    public bool GetChargable(AttackType type, int comboStage)
    {
        var param = GetAtkParams(type);
        if (param == null || comboStage < 0 || comboStage >= param.moveDistances.Count)
        {
            Debug.LogError($"Weapon {weaponName} doesn't have {type} stage {comboStage} chargable!");
            return false;
        }
        return param.isChargable[comboStage];
    }
    
    public float GetRotateSpeed(AttackType type, int comboStage)
    {
        var param = GetAtkParams(type);
        if (param == null || comboStage < 0 || comboStage >= param.moveDistances.Count)
        {
            Debug.LogError($"Weapon {weaponName} doesn't have {type} stage {comboStage} rotate speed!");
            return 0f;
        }
        return param.rotateSpeeds[comboStage];
    }

}