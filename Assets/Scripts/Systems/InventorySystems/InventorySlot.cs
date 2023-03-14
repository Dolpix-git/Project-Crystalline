using System;
using UnityEngine;

[Serializable]
public class InventorySlot{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount;

    public ItemData ItemData { get => itemData; }
    public int Amount { get => amount; }

    public InventorySlot(ItemData itemData) {
        this.itemData = itemData;
        amount = 0;
    }

    public InventorySlot() {
        itemData = null;
        amount = 0;
    }

    /// <summary>
    /// returns true if there is a remaining amount
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="remaning"></param>
    /// <returns></returns>
    public bool AddToStack(int amount, out int remaning) {
        this.amount += amount;
        remaning = this.amount - ItemData.MaxStackSize;
        this.amount = Mathf.Clamp(this.amount,0,itemData.MaxStackSize);
        remaning = Mathf.Max(0, remaning);

        if (remaning == 0) {
            return false;
        } else {
            return true;
        }
    }
    /// <summary>
    /// returns true if there is a remaining amount
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="remaning"></param>
    /// <returns></returns>
    public bool RemoveFromStack(int amount, out int remaning) {
        this.amount -= amount;
        remaning = Mathf.Min(this.amount, 0);
        this.amount = Mathf.Clamp(this.amount, 0, itemData.MaxStackSize);
        remaning *= -1;

        if (remaning == 0) {
            return false;
        } else {
            return true;
        }
    }
    /// <summary>
    /// Warning, will reset amount
    /// </summary>
    /// <param name="itemData"></param>
    public void SetItemData(ItemData itemData) {
        this.itemData = itemData;
        amount = 0;
    }
}
