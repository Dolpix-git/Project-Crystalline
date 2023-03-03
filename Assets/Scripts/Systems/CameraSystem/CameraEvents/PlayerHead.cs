using FishNet.Object;
using System;
using UnityEngine;

public class PlayerHead : NetworkBehaviour {
    /// <summary>
    /// Activated when client creates and boots a camera.
    /// </summary>
    public static event Action<Transform> OnFirstPersonCamera;
    /// <summary>
    /// The head of the player where the camera should be positioned
    /// </summary>
    public Transform head;
    public override void OnStartClient() {
        base.OnStartClient();

        // Check if we are the owner, then send the head transform refrence.
        if (base.IsOwner) {
            NetworkObject nob = base.LocalConnection.FirstObject;
            if (nob == base.NetworkObject) {
                OnFirstPersonCamera?.Invoke(head);
            }
        }
    }
    public Vector3 GetPlayerForwardVector3() {
        return GetComponent<PlayerNetworker>().LastMove.CameraForward;
    }
}
