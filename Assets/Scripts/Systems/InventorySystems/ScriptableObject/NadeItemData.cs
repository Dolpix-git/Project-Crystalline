using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "NadeItemData", menuName = "InventoryItem/NadeItemData", order = 1)]
public class NadeItemData : ItemData {
    [SerializeField] private GameObject nadePrefab;
    [SerializeField] private float nadeStartDelay;
    [SerializeField] private float nadeThrowForce;
    public override bool Activate(Vector3 forward, float time, InventorySystem ctx, int index) {
        if (time < nadeStartDelay) return true;

        int amountToActivate = 1;
        if (ctx.AttemptToRemoveAtIndex(index, amountToActivate, out int remaning)) return true;

        Vector3 position = ctx.transform.position + (ctx.transform.forward * 2f);
        forward = (forward + Vector3.up).normalized;
        Quaternion rotation = Quaternion.LookRotation(forward);
        GameObject result = ctx.SpawnObject(nadePrefab, position, rotation);

        IThrowable throwable = result.GetComponent<IThrowable>();
        throwable.Initialize(forward * nadeThrowForce, ctx.Owner);

        return false;
    }
}