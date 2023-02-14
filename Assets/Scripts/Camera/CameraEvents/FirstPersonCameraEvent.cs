using FishNet.Object;
using System;
using UnityEngine;

public class FirstPersonCameraEvent : NetworkBehaviour {
    public static event Action<Transform> OnFirstPersonCamera;

    public override void OnStartClient() {
        base.OnStartClient();
        if (base.IsOwner) {
            NetworkObject nob = base.LocalConnection.FirstObject;
            if (nob == base.NetworkObject) {
                OnFirstPersonCamera?.Invoke(transform);
            }
        }
    }
}
