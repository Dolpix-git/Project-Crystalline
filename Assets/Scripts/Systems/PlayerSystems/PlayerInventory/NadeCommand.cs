using UnityEngine;

public class NadeCommand : Command {
    [SerializeField] private NadeCommandScriptableObject nade;
    private PlayerWeaponManager refrence;
    private int amount;

    public void Setup(NadeCommandScriptableObject nade, PlayerWeaponManager refrence, int amount) {
        this.nade = nade;
        this.refrence = refrence;
        this.amount = amount;
    }

    public override void Activate(Vector3 lookVector) {
        if (refrence.disabled) { return; }

        Vector3 position = refrence.transform.position + (refrence.transform.forward * 2f);
        Vector3 forward = lookVector + Vector3.up;
        Quaternion rotation = Quaternion.LookRotation(forward);
        GameObject result = refrence.SpawnObject(nade.NadePrefab, position, rotation);

        IThrowable throwable = result.GetComponent<IThrowable>();
        throwable.Initialize(forward * nade.ThrowForce, refrence.Owner);
    }
}
