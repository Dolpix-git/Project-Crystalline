using UnityEngine;

public class NadeCommand : Command {
    [SerializeField] private NadeCommandScriptableObject nade;
    private PlayerWeaponManager refrence;

    public void Setup(NadeCommandScriptableObject nade, PlayerWeaponManager refrence, int amount, int inventoryIndex) {
        this.nade = nade;
        this.refrence = refrence;
        this.amount = amount;
        this.inventoryIndex = inventoryIndex;
    }
    public override int AttemptToAddAmount(int amount) {
        this.amount += amount;
        int overflow = this.amount - nade.MaxStack;
        this.amount = Mathf.Clamp(this.amount, 0, nade.MaxStack);
        overflow = Mathf.Max(0,overflow);
        return overflow;
    }
    public override void Activate(Vector3 lookVector) {
        if (refrence.disabled) { return; }
        if (amount <= 0) {
            refrence.RemoveObjectFromInventory(inventoryIndex);
        }
        amount--;

        Vector3 position = refrence.transform.position + (refrence.transform.forward * 2f);
        Vector3 forward = lookVector + Vector3.up;
        Quaternion rotation = Quaternion.LookRotation(forward);
        GameObject result = refrence.SpawnObject(nade.NadePrefab, position, rotation);

        IThrowable throwable = result.GetComponent<IThrowable>();
        throwable.Initialize(forward * nade.ThrowForce, refrence.Owner);
    }
    public override void Drop(Vector3 lookVector) {
        Vector3 position = refrence.transform.position + (refrence.transform.forward * 2f);
        Vector3 forward = lookVector;
        Quaternion rotation = Quaternion.LookRotation(forward);
        GameObject result = refrence.SpawnObject(nade.NadeObject, position, rotation);

        // Get the droped object and give it an amount to store, so when its picked back up it keeps its continuity

        refrence.RemoveObjectFromInventory(inventoryIndex);
    }
}
