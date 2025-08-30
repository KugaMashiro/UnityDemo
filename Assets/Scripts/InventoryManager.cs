using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData itemData;

    public uint currentCount;
    //public int maxStack;

    public InventoryItem(ItemData data, uint initialCnt = 0)
    {
        itemData = data;
        if (initialCnt == 0)
        {
            currentCount = data.maxStack;
        }
        else
        {
            currentCount = (uint)Mathf.Min(initialCnt, data.maxStack);
        }
    }
}



public class InventoryManager : MonoBehaviour
{
    [Header("Player items")]
    [SerializeField] private List<InventoryItem> _playerItems;
    private int _currentItemIndex = 0;
    public int CurrentItemIndex => _currentItemIndex;
    public InventoryItem CurrentItem => _playerItems.Count > 0
        ? _playerItems[_currentItemIndex]
        : null;


    public void AddItem(ItemData data, uint count = 1)
    {
        var existingItem = _playerItems.Find(item => item.itemData == data);
        if (existingItem != null)
        {
            uint addable = data.maxStack - existingItem.currentCount;
            existingItem.currentCount += (uint)Mathf.Min(count, addable);
        }
        else
        {
            _playerItems.Add(new InventoryItem(data, count));
        }
    }

    public bool ConsumeItem(ItemData data, uint count = 1)
    {
        var item = _playerItems.Find(i => i.itemData = data);
        if (item == null || item.currentCount < count) return false;

        item.currentCount -= count;
        return true;
        //if()
    }

    public bool CanConsumeItem(ItemData data, uint count = 1)
    {
        var item = _playerItems.Find(i => i.itemData = data);
        if (item == null || item.currentCount < count) return false;
        return true;
    }

    public bool ConsumeItem(uint count=1)
    {
        var item = _playerItems[_currentItemIndex];
        if (item.currentCount < count) return false;
        item.currentCount -= count;
        return true;
    }

    public bool CanConsumeItem(uint count=1)
    {
        var item = _playerItems[_currentItemIndex];
        if (item.currentCount < count) return false;
        return true;
    }

}
