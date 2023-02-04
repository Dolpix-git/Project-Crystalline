using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public class PlayerNetworker : NetworkBehaviour {
    private Rigidbody rigidBody;
    public Rigidbody RigidBody { get => rigidBody; set => rigidBody = value; }

    private void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
    }
    private void OnDestroy() {
        if (InstanceFinder.TimeManager != null) {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    private void TimeManager_OnTick() {
        if (base.IsOwner) {
            Reconciliation(default, false);
            CheckInput(out PlayerMoveData md);
            Move(md, false);
        }
        if (base.IsServer) {
            Move(default, true);
        }
    }

    private void TimeManager_OnPostTick() {
        if (base.IsServer) {
            PlayerReconcileData rd = new PlayerReconcileData(transform.position, transform.rotation, rigidBody.velocity, rigidBody.angularVelocity);
            Reconciliation(rd, true);
        }
    }

    [Replicate]
    private void Move(PlayerMoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false) {
        // Update State
    }

    [Reconcile]
    private void Reconciliation(PlayerReconcileData rd, bool asServer, Channel channel = Channel.Unreliable) {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
        rigidBody.velocity = rd.Velocity;
        rigidBody.angularVelocity = rd.AngularVelocity;
    }
}
