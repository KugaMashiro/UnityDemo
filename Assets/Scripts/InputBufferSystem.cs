using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
public enum BufferedInputType
{
    Roll,
    AttackLight,
    AttackHeavy,
}

public enum InputPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public class InputBufferItem
{
    private static uint _nextId = 1;
    private static readonly object _idLock = new object();

    public uint UniqueId { get; }

    public BufferedInputType InputType { get; }
    public float Timestamp { get; }
    public Vector3 BufferedDir { get; }
    public bool IsProcessed { get; set; }
    public InputPriority Priority { get; }

    public InputBufferItem(BufferedInputType type, InputPrioritySettings prioritySettings, Vector3 dir)
    {
        lock (_idLock)
        {
            UniqueId = _nextId++;
            if (_nextId > uint.MaxValue - 1000)
                _nextId = 1;
        }

        InputType = type;
        Timestamp = UnityEngine.Time.time;
        IsProcessed = false;
        Priority = prioritySettings.GetPriority(type);
        BufferedDir = dir;
    }
}

public class InputBufferSystem : MonoBehaviour
{
    public static InputBufferSystem Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float bufferExpireTime = 0.2f;
    [SerializeField] private InputPrioritySettings prioritySettings;
    [SerializeField] private int maxBufferLength = 100;
    [SerializeField] private bool prioritizeNewestSamePriority = true;

    private readonly Queue<InputBufferItem> _inputBuffer = new Queue<InputBufferItem>();
    private readonly Dictionary<uint, InputBufferItem> _idToItemMap = new Dictionary<uint, InputBufferItem>();

    public event Action OnBufferUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (prioritySettings == null)
        {
            Debug.LogError("InputPrioritySettings not assigned!");
        }
    }

    private void Update()
    {
        CleanExpiredInputs();
    }

    public uint AddInput(BufferedInputType inputType, Vector3 curMoveInput)
    {
        if (prioritySettings == null) return 0;
        var inputItem = new InputBufferItem(inputType, prioritySettings, curMoveInput);

        while (_inputBuffer.Count >= maxBufferLength)
        {
            var oldestInput = _inputBuffer.Dequeue();
            _idToItemMap.Remove(oldestInput.UniqueId);
        }

        _inputBuffer.Enqueue(inputItem);
        _idToItemMap[inputItem.UniqueId] = inputItem;
        //OnBufferUpdated?.Invoke();

        //Debug.Log($"InputBuffer: {inputItem.UniqueId} added.");
        return inputItem.UniqueId;
    }

    private void CleanExpiredInputs()
    {
        while (_inputBuffer.Count > 0)
        {
            var oldestInput = _inputBuffer.Peek();
            if (Time.time - oldestInput.Timestamp > bufferExpireTime)
            {
                _inputBuffer.Dequeue();
                _idToItemMap.Remove(oldestInput.UniqueId);
                //Debug.Log($"InputBuffer: {oldestInput.UniqueId} expired, {Time.time}, {bufferExpireTime}.");
            }
            else break;
        }
    }

    public InputBufferItem GetValidInput(List<BufferedInputType> allowedTypes)
    {
        CleanExpiredInputs();

        var validInputs = _inputBuffer
            .Where(input => !input.IsProcessed && allowedTypes.Contains(input.InputType))
            .ToList();

        if (validInputs.Count == 0) return null;

        var sortedInputs = validInputs
            .OrderByDescending(input => input.Priority)
            .ThenByDescending(input => prioritizeNewestSamePriority ? input.Timestamp : -input.Timestamp);

        return validInputs.First();
    }

    public void ConsumeInputItem(uint itemIdToConsume)
    {
        if (_idToItemMap.TryGetValue(itemIdToConsume, out InputBufferItem item))
        {
            //Debug.Log($"InputBuffer: {item.UniqueId} consumed.");
            if (item.IsProcessed) return;
            item.IsProcessed = true;
        }
        //ClearProcessedInputs();
    }

    private void ClearProcessedInputs()
    {
        // var remainingInputs = _inputBuffer.Where(input => !input.IsProcessed);
        // _inputBuffer.Clear();

        // foreach (var input in remainingInputs)
        // {
        //     _inputBuffer.Enqueue(input);
        // }

        // foreach (var item in )
        var invalidItems = _inputBuffer.Where(item => !IsItemValid(item)).ToList();
        var validItems = _inputBuffer.Where(item => IsItemValid(item)).ToList();

        _inputBuffer.Clear();
        foreach (var item in validItems)
        {
            _inputBuffer.Enqueue(item);
        }
        foreach (var item in invalidItems)
        {
            _idToItemMap.Remove(item.UniqueId);
        }
    }

    private bool IsItemValid(InputBufferItem item)
    {
        return !item.IsProcessed && (Time.time - item.Timestamp) <= bufferExpireTime;
    } 

    public void ClearAllBufferedInput()
    {
        _inputBuffer.Clear();
    }

}
