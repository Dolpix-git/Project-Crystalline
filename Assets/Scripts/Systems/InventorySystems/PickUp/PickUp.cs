using FishNet.Object;
using UnityEngine;

public class PickUp : NetworkBehaviour, IPickupable, IRound {
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount;
    public int GetItemAmount() {
        return Mathf.Clamp(amount,0,itemData.MaxStackSize);
    }
    public ItemData GetItemData() {
        return itemData;
    }


    [Server]
    public void SetAmount(int amount) {
        this.amount = amount;
        if (amount <= 0) base.Despawn();
    }
    [Server]
    public void SetItem(ItemData itemData) {
        this.itemData = itemData;
    }
    [Server]
    public void RoundEnded() {
        Despawn();
    }
}
