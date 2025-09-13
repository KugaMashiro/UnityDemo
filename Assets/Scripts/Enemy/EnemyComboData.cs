using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComboCode
{
    LightSingle = 0,
    LightSwift = 1,
    HeavyStrike = 2
}


[CreateAssetMenu(fileName = "EnemyAttackCombo", menuName = "Enemy/EnemyComboData")]
public class EnemyComboData : ScriptableObject
{
    [Header("ComboName")]
    public string comboName;

    [Header("Combo Code")]
    public ComboCode comboCode;

    [Header("Combo Cool Down")]
    public float comboCoolDown;

    [Header("Combo Rigidity")]
    public float comboRigidity;

    [Header("Combo Weight")]
    public int comboWeight;

    //[Header("Combo Short Hash")]
    public int ComboShortHash { get => Animator.StringToHash(comboName); }
}
