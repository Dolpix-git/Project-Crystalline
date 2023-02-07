using FishNet;
using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Serializing;
using FishNet.Transporting;
using System;
using System.IO;
using UnityEngine;

public class PlayerNetworker : NetworkBehaviour {
    private Rigidbody rigidBody;
    private CapsuleCollider capsuleCollider;
    private PlayerStateMachine playerStateMachine;
    public Rigidbody RigidBody { get => rigidBody; set => rigidBody = value; }
    public CapsuleCollider CapsuleCollider { get => capsuleCollider; set => capsuleCollider = value; }

    private void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerStateMachine = new PlayerStateMachine(this);

        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
    }
    private void OnDestroy() {
        if (InstanceFinder.TimeManager != null) {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        playerStateMachine.EvaluateCollision(collision);
    }
    private void OnCollisionStay(Collision collision) {
        playerStateMachine.EvaluateCollision(collision);
    }

    private void TimeManager_OnTick() {
        if (base.IsOwner) {
            Reconciliation(default, false);
            PlayerInputManager.Instance.GetCharacterControllerInputs(out PlayerMoveData md);
            Move(md, false);
        }
        if (base.IsServer) {
            Move(default, true);
        }
    }

    private void TimeManager_OnPostTick() {
        if (base.IsServer) {
            PlayerReconcileData rd = new PlayerReconcileData(transform.position, transform.rotation, rigidBody.velocity, rigidBody.angularVelocity, playerStateMachine.CurrentState.PlayerState(), playerStateMachine.CurrentState.CurrentSubState.PlayerState());
            Reconciliation(rd, true);
        }
    }


    [Replicate]
    private void Move(PlayerMoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false) {
        playerStateMachine.UpdateStates(md);
    }

    [Reconcile]
    private void Reconciliation(PlayerReconcileData rd, bool asServer, Channel channel = Channel.Unreliable) {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
        rigidBody.velocity = rd.Velocity;
        rigidBody.angularVelocity = rd.AngularVelocity;
        playerStateMachine.CurrentState = playerStateMachine.States.GetState(rd.SuperPlayerState);
        playerStateMachine.CurrentState.EnterState();
        playerStateMachine.CurrentState.SetSubStateReconsile(rd.SubPlayerState);
    }
}