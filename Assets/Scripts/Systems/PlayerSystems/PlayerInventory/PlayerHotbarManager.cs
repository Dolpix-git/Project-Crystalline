using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerHotbarManager : InventorySystem{
    [SyncVar] private int currentInventoryIndex;
    private float lastActivateTime;
    private float lastDropTime;

    //Too doo: add client side requests to server
    private void Update() {
        if (!IsOwner) { return; }

        if (PlayerInputManager.Instance.GetWeaponsInput()) {
            Activate(PlayerInputManager.Instance.GetCamForward());
        } 
    }


    [ServerRpc]
    public void Drop(Vector3 forward) {
        Log.LogMsg(LogCategories.PlayerHotbar, $"SERVER: We recived a request to drop an item at index {currentInventoryIndex}");
        if (inventory[currentInventoryIndex].ItemData == null) return;
        if (!inventory[currentInventoryIndex].ItemData.DropAmount(forward, Time.time - lastDropTime, this, inventory[currentInventoryIndex].Amount, currentInventoryIndex))
            lastDropTime = Time.time;
    }
    [ServerRpc]
    public void Activate(Vector3 forward) {
        Log.LogMsg(LogCategories.PlayerHotbar, $"SERVER: We recived a request to activate an item at index {currentInventoryIndex}");
        if (inventory[currentInventoryIndex].ItemData == null) return;
        if (!inventory[currentInventoryIndex].ItemData.Activate(forward, Time.time - lastActivateTime, this, currentInventoryIndex))
            lastActivateTime = Time.time;
    }
    [ServerRpc]
    public void ChangeIndex(int index) {
        Log.LogMsg(LogCategories.PlayerHotbar, $"SERVER: We recived a request to change current index too {index}");
        // too doo, if index is out of range please dont change the index
        currentInventoryIndex = index;
    }
}
