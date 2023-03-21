using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerHotbarManager : InventorySystem{
    [SyncVar] private int currentInventoryIndex;
    private float lastActivateTime;
    private float lastDropTime;
    private PlayerHealth playerHealth;
    private PlayerInteraction playerInteraction;

    private void Awake() {
        playerHealth = GetComponent<PlayerHealth>();
        playerInteraction = GetComponent<PlayerInteraction>();

        playerHealth.OnDeath += PlayerHealth_OnDeath;
        playerHealth.OnDisabled += PlayerHealth_OnDisabled;
        PlayerInputManager.Instance.OnThrow += Instance_OnThrow;
        PlayerInputManager.Instance.OnDrop += Instance_OnDrop;
        PlayerInputManager.Instance.OnHotbarChange += Instance_OnHotbarChange;
        PlayerInputManager.Instance.OnHotbarDelta += Instance_OnHotbarDelta;
    }

    private void OnDestroy() {
        playerHealth.OnDeath -= PlayerHealth_OnDeath;
        playerHealth.OnDisabled -= PlayerHealth_OnDisabled;
        PlayerInputManager.Instance.OnThrow -= Instance_OnThrow;
        PlayerInputManager.Instance.OnDrop -= Instance_OnDrop;
        PlayerInputManager.Instance.OnHotbarChange -= Instance_OnHotbarChange;
        PlayerInputManager.Instance.OnHotbarDelta -= Instance_OnHotbarDelta;
    }


    [Client]
    private void Instance_OnThrow() {
        if (!IsOwner || UIManager.Instance.IsMenu) { return; }
        Activate(PlayerInputManager.Instance.CamForward);
    }
    [Client]
    private void Instance_OnDrop() {
        if (!IsOwner || UIManager.Instance.IsMenu) { return; }
        Drop(PlayerInputManager.Instance.CamForward);
    }
    [Client]
    private void Instance_OnHotbarChange(int obj) {
        if (!IsOwner || UIManager.Instance.IsMenu) { return; }
        ChangeIndex(obj);
    }
    [Client]
    private void Instance_OnHotbarDelta(int obj) {
        if (!IsOwner || UIManager.Instance.IsMenu) { return; }
        ChangeIndexDelta(obj);
    }

    [ServerRpc]
    public void Drop(Vector3 forward) {
        if (playerHealth.IsDisabled || playerHealth.IsDead || playerInteraction.IsInteractionPause) return;
        if (inventory[currentInventoryIndex].ItemData == null) return;

        Log.LogMsg(LogCategories.PlayerHotbar, $"SERVER: We recived a request to drop an item at index {currentInventoryIndex}");
        if (!inventory[currentInventoryIndex].ItemData.DropAmount(forward, Time.time - lastDropTime, this, inventory[currentInventoryIndex].Amount, currentInventoryIndex))
            lastDropTime = Time.time;
    }
    [ServerRpc]
    public void Activate(Vector3 forward) {
        if (playerHealth.IsDisabled || playerHealth.IsDead || playerInteraction.IsInteractionPause) return;
        if (inventory[currentInventoryIndex].ItemData == null) return;

        Log.LogMsg(LogCategories.PlayerHotbar, $"SERVER: We recived a request to activate an item at index {currentInventoryIndex}");
        if (!inventory[currentInventoryIndex].ItemData.Activate(forward, Time.time - lastActivateTime, this, currentInventoryIndex))
            lastActivateTime = Time.time;
    }
    [ServerRpc]
    public void ChangeIndex(int index) {
        if (playerHealth.IsDisabled || playerHealth.IsDead || playerInteraction.IsInteractionPause) return;
        Log.LogMsg(LogCategories.PlayerHotbar, $"SERVER: We recived a request to change current index too {index}");
        currentInventoryIndex = Mathf.Clamp(index, 0, inventory.Length - 1);
    }
    [ServerRpc]
    public void ChangeIndexDelta(int delta) {
        if (playerHealth.IsDisabled || playerHealth.IsDead || playerInteraction.IsInteractionPause) return;
        Log.LogMsg(LogCategories.PlayerHotbar, $"SERVER: We recived a request to change current index by {delta}");
        currentInventoryIndex = Mathf.Clamp(currentInventoryIndex+delta, 0, inventory.Length-1);
    }

    [Server]
    private void PlayerHealth_OnDisabled() {
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].ItemData == null) continue;
            inventory[i].ItemData.DropAmount(transform.forward, 1000, this, inventory[i].Amount, i);
        }
    }
    [Server]
    private void PlayerHealth_OnDeath() {
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].ItemData == null) continue;
            inventory[i].ItemData.DropAmount(transform.forward, 1000, this, inventory[i].Amount, i);
        }
    }
    [ServerRpc]
    public void PerchaseItem(int itemIndex, int amount) {
        if (!CheckZone(gameObject.transform.position)) return;
        EconomyManager.Instance.PerchaseItem(BuyMenuUI.Instance.itemToBuy[itemIndex], amount, BuyMenuUI.Instance.itemToBuy[itemIndex].Cost, Owner);
    }

    private bool CheckZone(Vector3 pos) {
        Collider[] hits = Physics.OverlapSphere(pos, 0.1f);
        for (int i = 0; i < hits.Length; i++) {
            IZonable zone = hits[i].GetComponent<IZonable>();
            if (zone == null) continue;

            if (zone.GetZone() != Zones.Buy) continue;

            return true;
        }
        return false;
    }
}
