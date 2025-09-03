using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Player Properties")]
    [SerializeField] private int _maxHealthPoint = 500;
    [SerializeField] private int _maxStaminaPoint = 300;
    [SerializeField] private int _rollStaminaCost = 50;
    public int RollStaminaCost => _rollStaminaCost;
    [SerializeField] private int _staminaRecoverPerSecond = 60;
    public int StaminaRecoverPerSecond => _staminaRecoverPerSecond;

    [SerializeField] public float RollDistance { get; } = 3f;
    [SerializeField] public float JumpBackDistance { get; } = 1f;
    public float FaceRotateSpeed { get; private set; } = 100f;
    public float WalkSpeed { get; private set; } = 3f;
    public float RunSpeed { get; private set; } = 5f;




    [SerializeField] private int _curHealthPoint;
    [SerializeField] private int _curStaminaPoint;
    private bool _isInvincible;
    private bool _canInteract = true;

    public int CurHealthPoint => _curHealthPoint;
    public int CurStaminaPoint => _curStaminaPoint;
    public bool IsInvincible => _isInvincible;
    public bool CanInteract => _canInteract;

    private void Awake()
    {
        _curHealthPoint = _maxHealthPoint;
        _curStaminaPoint = _maxStaminaPoint;
    }

    public void RecoverHealth(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Passing Nagetive Recover HP!");
            return;
        }

        int finalHP = Mathf.Min(_curHealthPoint + value, _maxHealthPoint);
        if (finalHP != _curHealthPoint)
        {
            _curHealthPoint = finalHP;
            EventCenter.PublishHealthRecover(_curHealthPoint);
        }
    }

    public void DecreaseHealth(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Passing Nagetive Decrease HP!");
            return;
        }

        int finalHP = Mathf.Max(_curHealthPoint - value, 0);
        if (finalHP != _curHealthPoint)
        {
            _curHealthPoint = finalHP;
            EventCenter.PublishHealthDecrease(_curHealthPoint);
        }
    }

    public void RecoverStamina(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Passing Nagetive Recover SP!");
            return;
        }

        int finalSP = Mathf.Min(_curStaminaPoint + value, _maxStaminaPoint);
        if (finalSP != _curStaminaPoint)
        {
            _curStaminaPoint = finalSP;
            EventCenter.PublishStaminaRecover(_curStaminaPoint);
        }
    }

    public void DecreaseStamina(int value)
    {
        if (value < 0)
        {
            Debug.LogError("Passing Nagetive Decrease HP!");
            return;
        }

        int finalSP = Mathf.Max(_curStaminaPoint - value, 0);
        if (finalSP != _curStaminaPoint)
        {
            _curStaminaPoint = finalSP;
            EventCenter.PublishStaminaRecover(_curStaminaPoint);
        }
    }

}
