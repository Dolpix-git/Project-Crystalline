using FishNet.Managing.Timing;
using FishNet.Object;
using UnityEngine;

public class PlayerWeaponManager : NetworkBehaviour {
    [SerializeField] private GameObject throable;
    [SerializeField] private float force;

    private void Update() {
        if (PlayerInputManager.Instance.GetWeaponsInput() && base.IsOwner) {
            Fire(base.TimeManager.GetPreciseTick(TickType.Tick), transform.position + (transform.forward * 2f), PlayerInputManager.Instance.GetCamForward() + Vector3.up);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pt">PerciseTick percise tick of the time manager</param>
    /// <param name="position"></param>
    /// <param name="forward"></param>
    private void Fire(PreciseTick pt, Vector3 position, Vector3 forward) {
        Debug.Log("Fired");

        //GameObject result = Instantiate(throable, position, Quaternion.identity);
        //IThrowable throwable = result.GetComponent<IThrowable>();
        //throwable.Initialize(pt, forward * force);

        CmdFireBase(pt, position, forward);
    }


    [ServerRpc]
    private void CmdFireBase(PreciseTick pt, Vector3 position, Vector3 forward) {
        GameObject result = Instantiate(throable, position, Quaternion.identity);
        base.Spawn(result);
        IThrowable throwable = result.GetComponent<IThrowable>();
        throwable.Initialize(pt, forward * force);

    }
}