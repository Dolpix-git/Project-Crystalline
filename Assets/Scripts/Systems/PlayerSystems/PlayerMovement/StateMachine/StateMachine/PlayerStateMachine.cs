using FishNet;
using UnityEngine;
using FishNet.Serializing.Helping;


public class PlayerStateMachine{
    #region Private.
    /// <summary>
    /// A refrence to the current root state of the state machine.
    /// </summary>
    private PlayerBaseState currentState;
    /// <summary>
    /// A refrence to the player states cashe so we can switch between states.
    /// </summary>
    private PlayerStateCashe states; 
    /// <summary>
    /// A refrence to the player networking code.
    /// </summary>
    private PlayerNetworker playerNet;
    /// <summary>
    /// A new player state setup that is used to get the general condition of the controller, like grounded.
    /// </summary>
    private PlayerStateSetup playerStateSetup;
    /// <summary>
    /// The velocity we mess with then apply to the rigidbody.
    /// </summary>
    private Vector3 velocity;
    /// <summary>
    /// A local refrence to the move data key presses.
    /// </summary>
    private PlayerMoveData moveData;
    /// <summary>
    /// A local refrence to the local tick delta
    /// </summary>
    private float tickDelta;
    /// <summary>
    /// Saves the original player height so when GDSS we can return to original height. we opt to get the collider hight and not set it
    /// so that designers dont have to touch code to adjust player heights.
    /// </summary>
    private float originalPlayerHeight;
    #endregion
    #region States.
    public bool OnGround => playerStateSetup.groundContactCount > 0;
    public bool OnSteep => playerStateSetup.steepContactCount > 0;
    #endregion
    #region Getters and Setters.
    public Rigidbody RigidBody { get => playerNet.RigidBody; }
    public CapsuleCollider PlayerCollider { get => playerNet.CapsuleCollider; }
    public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }
    public PlayerStateCashe States { get => states; set => states = value; }
    public PlayerEffects PlayerEffects { get => playerNet.PlayerEffects; }
    public Vector3 Velocity { get => velocity; set { velocity = value; } }
    public PlayerMoveData MoveData { get => moveData;}
    public float TickDelta { get => tickDelta; set => tickDelta = value; }
    public Vector3 RightAxis { get => playerStateSetup.rightAxis; set => playerStateSetup.rightAxis= value; }
    public Vector3 ForwardAxis { get => playerStateSetup.forwardAxis; set => playerStateSetup.forwardAxis = value; }
    public Vector3 ContactNormal { get => playerStateSetup.contactNormal; set => playerStateSetup.contactNormal = value; }
    public Vector3 ConnectionVelocity { get => playerStateSetup.connectionVelocity; set => playerStateSetup.connectionVelocity = value; }
    public PlayerStateSetup PlayerStateSetup { get => playerStateSetup; set => playerStateSetup = value; }
    public float OriginalPlayerHeight { get => originalPlayerHeight; }
    #endregion

    public PlayerStateMachine(PlayerNetworker pn){
        playerNet = pn;
        States = new PlayerStateCashe(this);
        playerStateSetup = new PlayerStateSetup(this);
        currentState = States.Grounded();
        currentState.EnterState();
        originalPlayerHeight = pn.CapsuleCollider.height;
    }

    public void UpdateStates(PlayerMoveData md) {
        moveData = md;
        tickDelta = (float)InstanceFinder.TimeManager.TickDelta;

        playerStateSetup.Update();
        currentState.UpdateStates();
        RigidBody.velocity = velocity;

        if (!Comparers.IsDefault(moveData)) {
            playerNet.transform.rotation = Quaternion.LookRotation(ForwardAxis, Vector3.up);
        }

        playerStateSetup.ClearPlayerState();
    }

    public void SetPlayerMovementData(Vector2 move) {
        moveData.Movement = move;
    }
}


