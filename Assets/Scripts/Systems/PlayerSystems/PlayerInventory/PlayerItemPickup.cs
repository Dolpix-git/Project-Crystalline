using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(InventorySystem))]
public class PlayerItemPickup : NetworkBehaviour{
    private InventorySystem inventorySystem;
    private PlayerHealth playerHealth;
    private void Awake() {
        inventorySystem = GetComponent<InventorySystem>();
        playerHealth = GetComponent<PlayerHealth>();
    }
    private void OnCollisionEnter(Collision collision) {
        if (!base.IsServer) return;

        if (playerHealth.IsDisabled || playerHealth.IsDead) return;

        IPickupable pickup = collision.gameObject.GetComponent<IPickupable>();
        if (pickup == null) return;

        Log.LogMsg(LogCategories.PlayerItemPickup, $"Player picked up {pickup.GetItemData()} with amount:{pickup.GetItemAmount()}");
        inventorySystem.AtttemptToAddItem(pickup.GetItemData(), pickup.GetItemAmount(), out int remaning);
        Log.LogMsg(LogCategories.PlayerItemPickup, $"There was {remaning} reamaning");
        pickup.SetAmount(remaning);
    }
}
