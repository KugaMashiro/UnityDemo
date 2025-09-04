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
        if (FloatUtils.FloatEqual(e.MaxValue, 0))
        {
            Debug.LogError("devide 0 in updatehealth UI");
            return;
        }

        float sliderPercentage = e.CurValue / e.MaxValue;
        sliderPercentage = Mathf.Clamp01(sliderPercentage);

        _healthSlider.value = sliderPercentage;
    }

    public void UpdateStamina(PlayerStatusChangeEventArgs e)
    {
        //Debug.Log($"Update Stamina {e.Value}");
        if (FloatUtils.FloatEqual(e.MaxValue, 0))
        {
            Debug.LogError("devide 0 in updatestamina UI");
            return;
        }


        float sliderPercentage = e.CurValue / e.MaxValue;
        sliderPercentage = Mathf.Clamp01(sliderPercentage);

        _staminaSlider.value = sliderPercentage;
    }
}
