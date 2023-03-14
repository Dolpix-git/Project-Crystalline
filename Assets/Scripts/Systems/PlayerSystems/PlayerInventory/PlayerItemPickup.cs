using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(InventorySystem))]
public class PlayerItemPickup : NetworkBehaviour{
    InventorySystem inventorySystem;
    private void Awake() {
        inventorySystem = GetComponent<InventorySystem>();
        Log.LogMsg(LogCategories.PlayerItemPickup, $"Finding inventory system: {inventorySystem}");
    }
    private void OnCollisionEnter(Collision collision) {
        if (!base.IsServer) return;

        IPickupable pickup = collision.gameObject.GetComponent<IPickupable>();
        if (pickup == null) return;

        Log.LogMsg(LogCategories.PlayerItemPickup, $"Player picked up {pickup.GetItemData()} with amount:{pickup.GetItemAmount()}");
        inventorySystem.AtttemptToAddItem(pickup.GetItemData(), pickup.GetItemAmount(), out int remaning);
        Log.LogMsg(LogCategories.PlayerItemPickup, $"There was {remaning} reamaning");
        pickup.SetAmount(remaning);
    }
}
