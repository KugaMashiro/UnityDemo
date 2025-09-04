using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Player Properties")]
    [SerializeField] private float _maxHealthPoint = 500f;
    [SerializeField] private float _maxStaminaPoint = 100f;
    [SerializeField] private float _rollStaminaCost = 20f;

    [SerializeField] private float _normalStaminaDelta = 25f;
    [SerializeField] private float _runStaminaDelta = -10f;

    [SerializeField] public float RollDistance { get; } = 3f;
    [SerializeField] public float JumpBackDistance { get; } = 1f;
    public float FaceRotateSpeed { get; private set; } = 100f;
    public float WalkSpeed { get; private set; } = 3f;
    public float RunSpeed { get; private set; } = 5f;

    [SerializeField] private float _curHealthPoint;
    [SerializeField] private float _curStaminaPoint;
    private bool _isInvincible;
    private bool _canInteract = true;

    public float CurHealthPoint => _curHealthPoint;
    public float CurStaminaPoint => _curStaminaPoint;
    public bool IsInvincible => _isInvincible;
    public float RollStaminaCost => _rollStaminaCost;
    public float NormalStaminaDelta => _normalStaminaDelta;
    public float RunStaminaDelta => _runStaminaDelta;



    [SerializeField] private float _staminaDeltaPerSecond;
    public float StaminaDeltaPerSecond => _staminaDeltaPerSecond;
    public bool CanInteract => _canInteract;

    private void Awake()
    {
        _curHealthPoint = _maxHealthPoint;
        _curStaminaPoint = _maxStaminaPoint;
        _staminaDeltaPerSecond = 0f;
    }

    private void Update()
    {
        UpdateStamina();
    }

    private void UpdateStamina()
    {
        if (FloatUtils.FloatEqual(_staminaDeltaPerSecond, 0f))
        {
            return;
        }

        float deltaThisFrame = _staminaDeltaPerSecond * Time.deltaTime;
        float _newStamina = _curStaminaPoint + deltaThisFrame;
        _newStamina = Mathf.Clamp(_newStamina, 0f, _maxStaminaPoint);

        if (!FloatUtils.FloatEqual(_newStamina, _curStaminaPoint))
        {
            float delta = _newStamina - _curStaminaPoint;
            _curStaminaPoint = _newStamina;

            if (delta < 0f)
            {
                EventCenter.PublishStaminaDecrease(_curStaminaPoint, delta, _maxStaminaPoint);
            }

            else
            {
                EventCenter.PublishStaminaRecover(_curStaminaPoint, delta, _maxStaminaPoint);
            }
        }
    }

    public void SetStaminaDeltaPerSecond(float deltaPerSecond)
    {
        _staminaDeltaPerSecond = deltaPerSecond;
    }

    public void RecoverHealth(float value)
    {
        if (value < 0)
        {
            Debug.LogError("Passing Nagetive Recover HP!");
            return;
        }

        float finalHP = Mathf.Min(_curHealthPoint + value, _maxHealthPoint);
        //if (finalHP != _curHealthPoint)
        if (!FloatUtils.FloatEqual(finalHP, _curHealthPoint))
        {
            float delta = finalHP - _curHealthPoint;
            _curHealthPoint = finalHP;

            EventCenter.PublishHealthRecover(_curHealthPoint, delta, _maxHealthPoint);
        }
    }

    public void DecreaseHealth(float value)
    {
        if (value < 0)
        {
            Debug.LogError("Passing Nagetive Decrease HP!");
            return;
        }

        float finalHP = Mathf.Max(_curHealthPoint - value, 0);
        if (!FloatUtils.FloatEqual(finalHP, _curHealthPoint))
        {
            float delta = finalHP - _curHealthPoint;
            _curHealthPoint = finalHP;

            EventCenter.PublishHealthDecrease(_curHealthPoint, delta, _maxHealthPoint);
        }
    }

    public void RecoverStamina(float value)
    {
        if (value < 0)
        {
            Debug.LogError("Passing Nagetive Recover SP!");
            return;
        }

        float finalSP = Mathf.Min(_curStaminaPoint + value, _maxStaminaPoint);
        if (!FloatUtils.FloatEqual(finalSP, _curStaminaPoint))
        {
            float delta = finalSP - _curStaminaPoint;
            _curStaminaPoint = finalSP;

            EventCenter.PublishStaminaRecover(_curStaminaPoint, delta, _maxStaminaPoint);
        }
    }

    public void DecreaseStamina(float value)
    {
        if (value < 0)
        {
            Debug.LogError("Passing Nagetive Decrease HP!");
            return;
        }

        float finalSP = Mathf.Max(_curStaminaPoint - value, 0);
        if (!FloatUtils.FloatEqual(finalSP, _curStaminaPoint))
        {
            float delta = finalSP - _curStaminaPoint;
            _curStaminaPoint = finalSP;

            EventCenter.PublishStaminaDecrease(_curStaminaPoint, delta, _maxStaminaPoint);
        }
    }

}
