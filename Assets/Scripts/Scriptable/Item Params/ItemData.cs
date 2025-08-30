using UnityEngine;


public enum ItemType
{
    Drink = 0,
    Eat = 1,
    Buff = 2,
    Throw = 3,
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Player/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Item Name")]
    public string itemName;

    [Header("Item Type")]
    public ItemType itemType;

    [Header("Moveable")]
    public bool moveable;

    [Header("Should Lock")]
    public bool shouldLock;

    [Header("Move Speed")]
    public float moveSpeed;

    [Header("Move Blend Factor")]
    public float moveBlendFactor;

    [Header("Reuseable")]
    public bool reuseable;

    [Header("Max Stack")]
    public uint maxStack;


}
