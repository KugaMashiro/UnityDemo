using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class EventPool<T>: IPool where T : EventArgs, IPoolable, new()
{
    private readonly Stack<T> _pool = new Stack<T>();
    private readonly object _lock = new object();

    public int MaxSize { get; }

    public int CurrentCount
    {
        get
        {
            lock (_lock) { return _pool.Count; }
        }
    }

    public EventPool(int maxSize)
    {
        MaxSize = maxSize;
    }

    public T Get()
    {
        T item;
        lock (_lock)
        {
            item = _pool.Count > 0 ? _pool.Pop() : new T();

        }
        item.IsInUse = true;
        return item;
    }

    public void Release(T item)
    {
        if (!item.IsInUse) return;
        item.Reset();
        item.IsInUse = false;

        lock (_lock)
        {
            if (_pool.Count < MaxSize)
            {
                _pool.Push(item);
            }
        }
    }

    public void Shrink()
    {
        lock (_lock)
        {
            while (_pool.Count > MaxSize)
            {
                _pool.Pop();
            }
        }
    }

}

public class EventPoolManager
{
    private static readonly Lazy<EventPoolManager> _instance = new Lazy<EventPoolManager>(() => new EventPoolManager());
    public static EventPoolManager Instance => _instance.Value;

    private readonly Dictionary<Type, IPool> _pools = new Dictionary<Type, IPool>();
    private readonly Dictionary<Type, int> _sizeConfigs = new Dictionary<Type, int>();

    private const int DefaultMaxSize = 50;

    private EventPoolManager()
    {
        ConfigurePoolSize<MovementInputEventArgs>(500);
        ConfigurePoolSize<StateChangeEventArgs>(100);
    }

    private void ConfigurePoolSize<T>(int maxSize) where T : EventArgs, IPoolable, new()
    {
        if (maxSize < 0) throw new ArgumentException("Pool size can't be negative!");
        _sizeConfigs[typeof(T)] = maxSize;
    }

    public EventPool<T> GetPool<T>() where T : EventArgs, IPoolable, new()
    {
        Type type = typeof(T);
        if (!_pools.TryGetValue(type, out IPool pool))
        {
            int maxSize = _sizeConfigs.TryGetValue(type, out int size) ? size : DefaultMaxSize;
            pool = new EventPool<T>(maxSize);
            _pools[type] = pool;
        }
        return (EventPool<T>)pool;
    }

    public void ShrinkAllPools()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Shrink();
        }
    }
}