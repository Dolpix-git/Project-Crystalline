using UnityEngine;

[CreateAssetMenu(fileName = "SpikeItemData", menuName = "InventoryItem/SpikeItemData", order = 1)]
public class SpikeItemData : ItemData {
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private float spikeStartDelay;

    public override bool Activate(Vector3 forward, float time, InventorySystem ctx, int index) {
        if (time < spikeStartDelay) return true;

        int amountToActivate = 1;
        if (ctx.AttemptToRemoveAtIndex(index, amountToActivate, out int remaning)) return true;

        Vector3 position = ctx.transform.position + (ctx.transform.forward * 2f);
        forward = (forward + Vector3.up).normalized;
        Quaternion rotation = Quaternion.LookRotation(forward);
        GameObject result = ctx.SpawnObject(spikePrefab, position, rotation);

        result.GetComponent<Spike>().Initilised(ctx.Owner);

        return false;
    }
}