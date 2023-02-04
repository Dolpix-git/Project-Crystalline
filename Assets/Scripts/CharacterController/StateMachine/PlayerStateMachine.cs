using UnityEngine;

public class PlayerStateMachine{
    #region Serialized.
    [SerializeField]
    private float jumpForce = 15f;
    [SerializeField]
    private float moveRate = 15f;
    #endregion

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
    /// Velocity to apply to the rigidbody after the state machine is done running.
    /// </summary>
    private Vector3 velocity;
    /// <summary>
    /// A local refrence to the move data key presses.
    /// </summary>
    private PlayerMoveData moveData;
    #endregion

    #region Getters and Setters.
    public Rigidbody RigidBody { get => playerNet.RigidBody; set => playerNet.RigidBody = value; }
    public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }
    public Vector3 Velocity { get => velocity; set => velocity = value; }
    public PlayerMoveData MoveData { get => moveData; set => moveData = value; }
    #endregion

    public PlayerStateMachine(PlayerNetworker pn){
        states = new PlayerStateCashe(this);
        currentState = states.Grounded();
        currentState.EnterState();
        playerNet = pn;
    }

    public void UpdateState(PlayerMoveData md) {
        moveData = md;
        currentState.UpdateStates();
        RigidBody.velocity = velocity;
    }
}

