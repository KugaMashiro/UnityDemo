using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputPrioritySettings", menuName = "Input/Input Priority Settings")]
public class InputPrioritySettings : ScriptableObject
{
    [System.Serializable]
    public class InputPriorityPair
    {
        public BufferedInputType inputType;
        public InputPriority priority;
    }

    [SerializeField] private List<InputPriorityPair> priorityMappings;
    private Dictionary<BufferedInputType, InputPriority?> _temporaryOveerrides = new Dictionary<BufferedInputType, InputPriority?>();

    public InputPriority GetPriority(BufferedInputType inputType)
    {
        if (_temporaryOveerrides.TryGetValue(inputType, out var tempPriority) && tempPriority.HasValue)
        {
            return tempPriority.Value;
        }

        var pair = priorityMappings.FirstOrDefault(p => p.inputType == inputType);

        return pair != null ? pair.priority : InputPriority.Low;
    }

    public void SetTemporaryPriority(BufferedInputType inputType, InputPriority? priority)
    {
        _temporaryOveerrides[inputType] = priority;
    }

    public void ClearTemporaryPriority()
    {
        _temporaryOveerrides.Clear();
    }
}