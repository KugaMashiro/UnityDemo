using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDView : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Slider _healthSlider;
    [Header("Stamina Bar")]
    [SerializeField] private Slider _staminaSlider;

    private void OnEnable()
    {
        EventCenter.OnHealthRecover += UpdateHealth;
        EventCenter.OnHealthDecrease += UpdateHealth;
        EventCenter.OnStaminaRecover += UpdateStamina;
        EventCenter.OnStaminaDecrease += UpdateStamina;
    }

    private void OnDisable()
    {
        EventCenter.OnHealthRecover -= UpdateHealth;
        EventCenter.OnHealthDecrease -= UpdateHealth;
        EventCenter.OnStaminaRecover -= UpdateStamina;
        EventCenter.OnStaminaDecrease -= UpdateStamina;
    }


    public void UpdateHealth(PlayerStatusChangeEventArgs e)
    {
        _healthSlider.value = e.Value;
    }

    public void UpdateStamina(PlayerStatusChangeEventArgs e)
    {
        //Debug.Log($"Update Stamina {e.Value}");
        _staminaSlider.value = e.Value;
    }
}
