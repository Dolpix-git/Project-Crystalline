using FishNet.Object;
using UnityEngine;

public class PlayerItemPickup : NetworkBehaviour{
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private PlayerHealth playerHealth;

    private void OnTriggerEnter(Collider other) {
        if (!base.IsServer) return;

        if (playerHealth.IsDisabled || playerHealth.IsDead) return;

        IPickupable pickup = other.gameObject.GetComponent<IPickupable>();
        if (pickup == null) return;

        Log.LogMsg(LogCategories.PlayerItemPickup, $"Player picked up {pickup.GetItemData()} with amount:{pickup.GetItemAmount()}");
        inventorySystem.AtttemptToAddItem(pickup.GetItemData(), pickup.GetItemAmount(), out int remaning);
        Log.LogMsg(LogCategories.PlayerItemPickup, $"There was {remaning} reamaning");
        pickup.SetAmount(remaning);
    }
}
