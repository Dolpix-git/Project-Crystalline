using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "ItemData", menuName = "InventoryItem/ItemData", order = 1)]
public class ItemData : ScriptableObject{
    [SerializeField] private string itemName;
    [SerializeField] private int maxStackSize;
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private float dropDelay;
    [SerializeField] private int cost;
    [SerializeField] private Sprite itemTexture;

    public string ItemName { get => itemName; }
    public int MaxStackSize { get => maxStackSize; }
    public GameObject DropPrefab { get => dropPrefab; }
    public int Cost { get => cost; }

    public virtual bool Activate(Vector3 forward, float time, InventorySystem ctx, int index) {
        return false;
    }
    /// <summary>
    /// returns true if we were not able to exicute
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="time"></param>
    /// <param name="ctx"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public virtual bool DropAmount(Vector3 forward, float time, InventorySystem ctx, int amount, int index) {
        // if the drop delta time is less than the time then return true
        if (time < dropDelay) return true;

        ctx.AttemptToRemoveAtIndex(index, amount, out int remaninig);
        if (remaninig == amount) return true; // we were not able to remove any thing (weird? or a bug?)
        amount -= remaninig;

        Vector3 position = ctx.transform.position + (ctx.transform.forward * 2f);
        Vector3 lookVector = forward.normalized;
        Quaternion rotation = Quaternion.LookRotation(lookVector);
        GameObject result = ctx.SpawnObject(DropPrefab, position, rotation);

        IPickupable pickup = result.GetComponent<IPickupable>();
        pickup.SetItem(this);
        pickup.SetAmount(amount);

        
          
        return false;
    }
}
