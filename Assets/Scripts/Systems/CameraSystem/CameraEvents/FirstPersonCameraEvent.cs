using FishNet.Object;
using System;
using UnityEngine;

public class FirstPersonCameraEvent : NetworkBehaviour {
    /// <summary>
    /// Activated when client creates and boots a camera.
    /// </summary>
    public static event Action<Transform> OnFirstPersonCamera;

    public override void OnStartClient() {
        base.OnStartClient();

        // Check if we are the owner, then send the head transform refrence.
        if (base.IsOwner) {
            NetworkObject nob = base.LocalConnection.FirstObject;
            if (nob == base.NetworkObject) {
                OnFirstPersonCamera?.Invoke(transform);
            }
        }
    }
}
