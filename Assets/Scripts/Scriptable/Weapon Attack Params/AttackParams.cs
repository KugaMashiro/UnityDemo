using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AttackParams
{
    [Header("AttackType")]
    public AttackType attackType;

    [Header("Move Distance of Each Combo Stage")]
    public List<float> moveDistances = new();

    [Header("Max Combo")]
    public int maxComboCnt;

    [Header("Each Stage Chargable")]
    public List<bool> isChargable = new();

    [Header("Each Stage Rotate Speed")]
    public List<float> rotateSpeeds = new();
}