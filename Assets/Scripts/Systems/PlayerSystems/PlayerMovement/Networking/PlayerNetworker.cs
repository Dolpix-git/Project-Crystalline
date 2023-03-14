using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using System;
using UnityEngine;

public struct CachedStateInfo {
    public PlayerMoveData Move;
    public PlayerReconcileData Reconcile;
}

public class PlayerNetworker : NetworkBehaviour {
    #region SerializeFields.
    [Tooltip("The visual component of the character.")]
    [SerializeField] 
    private Transform visualTransform;

    [Tooltip("Duration to smooth desynchronizations over.")]
    [Range(0.01f, 0.5f)]
    public float smoothingDuration = 0.05f;
    #endregion

    #region Private.
    private Rigidbody rigidBody;
    private CapsuleCollider capsuleCollider;
    private PlayerStateMachine playerStateMachine;
    private PlayerEffects playerEffects;
    private Health playerHealth;

    private bool canMove = true;

    private PlayerMoveData lastMove;
    private CachedStateInfo? cachedStateInfo;
    private Vector3 instantiatedLocalPosition;
    private Quaternion instantiatedLocalRotation;
    private Vector3 smoothingPositionVelocity = Vector3.zero;
    private float smoothingRotationVelocity;
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    #endregion

    #region Getters and Setters.
    public Rigidbody RigidBody { get => rigidBody; set => rigidBody = value; }
    public CapsuleCollider CapsuleCollider { get => capsuleCollider; set => capsuleCollider = value; }
    public Transform VisualTransform { get => visualTransform; set => visualTransform = value; }
    public PlayerMoveData LastMove { get => lastMove; }
    public PlayerEffects PlayerEffects { get => playerEffects; }
    #endregion


    #region Start up.
    private void Awake() {
        AkSoundEngine.RegisterGameObj(gameObject);

        capsuleCollider = GetComponent<CapsuleCollider>();
        playerEffects = GetComponent<PlayerEffects>();
        rigidBody = GetComponent<Rigidbody>();
        playerHealth = GetComponent<Health>();
        playerStateMachine = new PlayerStateMachine(this);

        playerHealth.OnDeath += OnDeath;
        playerHealth.OnRespawned += OnRespawned;
        playerHealth.OnDisabled += OnDisabled;

        if (InstanceFinder.TimeManager is not null && InstanceFinder.PredictionManager is not null) {
            InstanceFinder.PredictionManager.OnPreReconcile += PredictionManager_OnPreReconcile;
            InstanceFinder.PredictionManager.OnPostReconcile += PredictionManager_OnPostReconcile;
            InstanceFinder.PredictionManager.OnPreReplicateReplay += PredictionManager_OnPreReplicateReplay;
            InstanceFinder.TimeManager.OnPreTick += TimeManager_OnPreTick;
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
        }
    }
    private void OnDestroy() {
        playerHealth.OnDeath -= OnDeath;
        playerHealth.OnRespawned -= OnRespawned;
        playerHealth.OnDisabled -= OnDisabled;

        if (InstanceFinder.TimeManager is not null && InstanceFinder.PredictionManager is not null) {
            InstanceFinder.PredictionManager.OnPreReconcile -= PredictionManager_OnPreReconcile;
            InstanceFinder.PredictionManager.OnPostReconcile -= PredictionManager_OnPostReconcile;
            InstanceFinder.PredictionManager.OnPreReplicateReplay -= PredictionManager_OnPreReplicateReplay;
            InstanceFinder.TimeManager.OnPreTick -= TimeManager_OnPreTick;
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }
    public override void OnStartNetwork() {
        base.OnStartNetwork();

        instantiatedLocalPosition = visualTransform.localPosition;
        instantiatedLocalRotation = visualTransform.localRotation;
    }
    public override void OnStartClient() {
        base.OnStartClient();
        if (!base.IsOwner && !IsServer) {
            RigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
    #endregion


    private void OnCollisionEnter(Collision collision) {
        playerStateMachine.PlayerStateSetup.EvaluateCollision(collision);
    }
    private void OnCollisionStay(Collision collision) {
        playerStateMachine.PlayerStateSetup.EvaluateCollision(collision);
    }
    private void Update() {
        MoveToTarget();
    }


    #region PredictionManager.
    private void PredictionManager_OnPreReplicateReplay(uint arg1, PhysicsScene arg2, PhysicsScene2D arg3) {
        if (!base.IsOwner && !base.IsServer) {
            // for non server and non owner - we want to setup the forces before fishnet calls physics simulate
            SimulateWithMove(lastMove);
        }
    }
    private void PredictionManager_OnPreReconcile(NetworkBehaviour obj) {
        // this is so we can restore the visual state to what it was and lerp to new visual position/rotation after reconcile is done
        SetPreviousTransformProperties();

        // if this is a non player vehicle we want to simulate it using last received data (if available)
        if (!base.IsOwner && !base.IsServer) {
            // reset to no move
            lastMove = default;

            // have we received info about the state of this vehicle on the server ?
            // if we haven't we just simulate from this point 
            if (cachedStateInfo.HasValue) {
                Reconcile(cachedStateInfo.Value.Reconcile);
                lastMove = cachedStateInfo.Value.Move;
            }
        }
    }
    private void PredictionManager_OnPostReconcile(NetworkBehaviour obj) {
        // Set transform back to where it was before reconcile so there's no visual disturbances.
        visualTransform.SetPositionAndRotation(previousPosition, previousRotation);
    }
    #endregion

    #region TimeManager.
    private void TimeManager_OnPreTick() {
        SetPreviousTransformProperties();
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

        if (!base.IsOwner && !base.IsServer) {
            SimulateWithMove(lastMove);
        }
    }
    private void TimeManager_OnPostTick() {
        if (base.IsServer) {
            PlayerReconcileData rd = new PlayerReconcileData(transform.position, transform.rotation, rigidBody.velocity, rigidBody.angularVelocity, capsuleCollider.center, capsuleCollider.height, playerStateMachine.CurrentState.PlayerState(), playerStateMachine.CurrentState.CurrentSubState.PlayerState());
            ObserversSendState(lastMove, rd);
            Reconciliation(rd, true);
        }
        ResetToTransformPreviousProperties();
    }
    #endregion

    #region Movement and Reconciliation.
    private void SimulateWithMove(PlayerMoveData md) {
        if (canMove) {
            playerStateMachine.UpdateStates(md);
        } else {
            playerStateMachine.RigidBody.velocity = Vector3.zero;
        }
    }
    [Replicate]
    private void Move(PlayerMoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false) {
        if (base.IsServer)
            lastMove = md;

        SimulateWithMove(md);
    }
    [Reconcile]
    private void Reconciliation(PlayerReconcileData rd, bool asServer, Channel channel = Channel.Unreliable) {
        Reconcile(rd);
    }
    private void Reconcile(PlayerReconcileData rd) {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
        rigidBody.velocity = rd.Velocity;
        rigidBody.angularVelocity = rd.AngularVelocity;
        capsuleCollider.center = rd.ColliderCenter;
        capsuleCollider.height = rd.ColliderHeight;
        playerStateMachine.CurrentState = playerStateMachine.States.GetState(rd.SuperPlayerState);
        playerStateMachine.CurrentState.EnterState();
        playerStateMachine.CurrentState.SetSubStateReconsile(rd.SubPlayerState);
    }

    [ObserversRpc(ExcludeOwner = true, BufferLast = true)]
    private void ObserversSendState(PlayerMoveData _lastMove, PlayerReconcileData _ReconcileData, Channel channel = Channel.Unreliable) {
        // ignore if we are controlling this vehicle (owner and server)
        if (!base.IsServer && !base.IsOwner) {
            // we just place in cache as this data is already old regards the client tick
            // so when the client reconcilates we will use this cache to fix up this vehicle position

            // store state in cache slot
            cachedStateInfo = new CachedStateInfo { Move = _lastMove, Reconcile = _ReconcileData };
        }
    }
    #endregion

    #region Visual Smoothing.
    private void MoveToTarget() {
        Transform t = visualTransform.transform;
        t.localPosition = Vector3.SmoothDamp(t.localPosition, instantiatedLocalPosition, ref smoothingPositionVelocity, smoothingDuration);
        t.localRotation = SmoothDampQuaternion(t.localRotation, instantiatedLocalRotation, ref smoothingRotationVelocity, smoothingDuration);
    }
    public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref float AngularVelocity, float smoothTime) {
        var delta = Quaternion.Angle(current, target);
        if (delta > 0.0f) {
            var t = Mathf.SmoothDampAngle(delta, 0.0f, ref AngularVelocity, smoothTime);
            t = 1.0f - t / delta;
            return Quaternion.Slerp(current, target, t);
        }

        return current;
    }
    private void SetPreviousTransformProperties() {
        previousPosition = visualTransform.position;
        previousRotation = visualTransform.rotation;
    }
    private void ResetToTransformPreviousProperties() {
        visualTransform.position = previousPosition;
        visualTransform.rotation = previousRotation;
    }
    #endregion

    #region Death and Respawn Events.
    private void OnDeath() {
        Log.LogMsg(LogCategories.Player, $"{gameObject.name} Has died");
        canMove = false;
    }
    private void OnRespawned() {
        Log.LogMsg(LogCategories.Player, $"{gameObject.name} Has been respawned");
        canMove = true;
        visualTransform.gameObject.SetActive(true);
        capsuleCollider.enabled = true;
        rigidBody.detectCollisions = true;
        rigidBody.useGravity = true;
    }
    private void OnDisabled() {
        Log.LogMsg(LogCategories.Player, $"{gameObject.name} Has been disabled");
        canMove = false;
        visualTransform.gameObject.SetActive(false);
        capsuleCollider.enabled = false;
        rigidBody.detectCollisions = false;
        rigidBody.useGravity = false;
    }
    #endregion
}