using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;


public class InventorySystem : NetworkBehaviour{
    [SerializeField] protected int inventorySize;

    [SyncVar] protected InventorySlot[] inventory;

    private void Start() {
        if (!base.IsServer) return;
        inventory = new InventorySlot[inventorySize];
        for (int i = 0; i < inventory.Length; i++) {
            inventory[i] = new();
        }
    }

    [Server]
    public bool AtttemptToAddItem(ItemData item, int amount, out int remaning) {
        
        // loop through inventory and find items that are the same, then attempt to fill there slots
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].ItemData == item) {
                inventory[i].AddToStack(amount, out amount);
                if (amount == 0) {
                    remaning = amount;
                    return false;
                }
            }
        }
        // loop through inventory and find free slots, then attempt to fill those slots
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].ItemData == null) {
                inventory[i].SetItemData(item);
                inventory[i].AddToStack(amount, out amount);
                if (amount == 0) {
                    remaning = amount;
                    return false;
                }
            }
        }
        // there is a remaining amount
        remaning = amount;
        return true;
    }
    [Server]
    public bool AttemptToAddAtIndex(int index, ItemData item, int amount, out int remaning) {
        if (index < 0 || index >= inventory.Length) { // Index out of range
            remaning = amount;
            return true;
        }
        if (inventory[index].ItemData == null) {
            inventory[index].SetItemData(item);
        } else if (inventory[index].ItemData != null) {
            remaning = amount;
            return true;
        }

        inventory[index].AddToStack(amount, out amount);

        if (amount == 0) { // we were able to add the amount
            remaning = 0;
            return false;
        } else { // there was some left over
            remaning = amount;
            return true;
        }
    }
    [Server]
    public void WipeItemFromInventory(ItemData item) {
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].ItemData == item) {
                inventory[i].SetItemData(null);
            }
        }
    }
    [Server]
    public bool AtttemptToRemoveItem(ItemData item, int amount, out int remaning) {
        // loop through inventory and find items that are the same, then attempt to fill there slots
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].ItemData == item) {
                inventory[i].RemoveFromStack(amount, out amount);
                if (amount > 0) { // We were able to remove but there is still some left over
                    inventory[i].SetItemData(null);
                } else { // We dont need to remove any more, so we can exit early
                    remaning = amount;
                    return false;
                }
            }
        }
        // there is a remaining amount
        remaning = amount;
        return true;
    }
    [Server]
    public bool AttemptToRemoveAtIndex(int index, int amount, out int remaning) {
        if (index < 0 || index >= inventory.Length) { // Index out of range
            remaning = amount;
            return true;
        }

        inventory[index].RemoveFromStack(amount, out amount);
        remaning = amount;

        if (amount > 0) { // there was some left over
            inventory[index].SetItemData(null);
            return true;
        }
        if (inventory[index].Amount == 0)
            inventory[index].SetItemData(null);

        return false;
    }

    [Server]
    public GameObject SpawnObject(GameObject obj, Vector3 position, Quaternion rotation) {
        GameObject result = Instantiate(obj, position, rotation);
        base.Spawn(result);
        return result;
    }
}
