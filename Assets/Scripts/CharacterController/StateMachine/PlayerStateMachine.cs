using FishNet;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using static UnityEditor.IMGUI.Controls.CapsuleBoundsHandle;
using UnityEngine.InputSystem;

public class PlayerStateMachine{
    private int groundContactCount, steepContactCount; // How many colliders are we touching and below the maxGroundAngle
    private Rigidbody connectedBody, previousConnectedBody; // all the rigidbodys of what we are colliding with. (provided with NULL if there isnt on)
    private Vector3 contactNormal, steepNormal; // The combination of all the normals that are considered ground or steep
    private Vector3 connectionVelocity; // Velocitys for applying to our rigidbody, this is for what we are standing on.
    private Vector3 connectionWorldPosition, connectionLocalPosition; // Vector3 for possitions for standing on moving objects.

    private float probeDistance = 0.1f; // the distance check for how far the controller will snap to a surface
    private LayerMask probeMask = -1;
    private float maxSnapSpeed = 100f; // the max speed of when the controller will not snap
    private float maxSlope = 40f;
    private float minDot;

    private int stepsSinceLastGrounded, stepsSinceLastJump; // Used to count the amount of physics steps we are not grounded for
    private int jumpPhase; // Counter for doubble jump

    #region States.
    public bool OnGround => groundContactCount > 0;
    public bool OnSteep => steepContactCount > 0;
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
    /// 
    /// </summary>
    private Vector3 velocity, desiredVelocity;
    private Vector3 rightAxis, forwardAxis;
    /// <summary>
    /// A local refrence to the move data key presses.
    /// </summary>
    private PlayerMoveData moveData;
    /// <summary>
    /// A local refrence to the local tick delta
    /// </summary>
    private float tickDelta;
    #endregion

    #region Getters and Setters.
    public Rigidbody RigidBody { get => playerNet.RigidBody; set => playerNet.RigidBody = value; }
    public CapsuleCollider pc { get => playerNet.CapsuleCollider; set => playerNet.CapsuleCollider = value; }
    public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }
    public PlayerStateCashe States { get => states; set => states = value; }
    public Vector3 Velocity { get => velocity; set => velocity = value; }
    public PlayerMoveData MoveData { get => moveData; set => moveData = value; }
    public float TickDelta { get => tickDelta; set => tickDelta = value; }
    public Vector3 RightAxis { get => rightAxis; set => rightAxis = value; }
    public Vector3 ForwardAxis { get => forwardAxis; set => forwardAxis = value; }
    public Vector3 ContactNormal { get => contactNormal; set => contactNormal = value; }
    public Vector3 ConnectionVelocity { get => connectionVelocity; set => connectionVelocity = value; }
    #endregion

    public PlayerStateMachine(PlayerNetworker pn){
        States = new PlayerStateCashe(this);
        currentState = States.Grounded();
        currentState.EnterState();
        playerNet = pn;

        minDot = Mathf.Cos(maxSlope * Mathf.Deg2Rad);
    }

    public void UpdateStates(PlayerMoveData md) {
        moveData = md;
        tickDelta = (float)InstanceFinder.TimeManager.TickDelta;

        UpdatePlayerState();
        PlayerInputs();

        currentState.UpdateStates();

        RigidBody.velocity = velocity;

        ClearPlayerState();
    }


    void PlayerInputs() {
        // This is to prevent clients from cheating, as they could in theory alter the vector2 on their machine
        moveData.Movement = Vector2.ClampMagnitude(moveData.Movement, 1f);

        rightAxis = PlayerFunctionHelpers.ProjectDirectionOnPlane(moveData.CameraRight, Vector3.up);
        forwardAxis = PlayerFunctionHelpers.ProjectDirectionOnPlane(moveData.CameraForward, Vector3.up);
    }

    /// <summary>
    /// Called from the networker that handels the monobehavior collisions.
    /// Handles the collisions for the grounded deffinitions.
    /// </summary>
    public void EvaluateCollision(Collision collision) {
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(Vector3.up, normal);
            if (upDot >= minDot) {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            } else if (upDot > -0.01f) {
                if (collision.gameObject.layer != 0) { return; }

                steepContactCount += 1;
                steepNormal += normal;
                if (groundContactCount == 0) {
                    connectedBody = collision.rigidbody;
                }
            }
        }
    }
    /// <summary>
    /// gets called at the begining and indexes loopes and resets data that needs to be reset, 
    /// this also handles when the collisions happen AKA snaping to ground
    /// </summary>
    void UpdatePlayerState() {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        // Sets the velocity of RB, normalises the contact normals, resets the jump counts
        velocity = RigidBody.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts()) {
            //wallRunUp = true;
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1) {
                jumpPhase = 0;
            }
            if (groundContactCount > 1) {
                contactNormal.Normalize();
            }
        }
        // If were in the air we would want to jump straight up not the direcion of the surface we were previously on
        else {
            contactNormal = Vector3.up;
        }
        if (connectedBody) {
            if (connectedBody.isKinematic || connectedBody.mass >= RigidBody.mass) {
                UpdateConnectionState();
            }
        }
    }

    /// <summary>
    /// clear state resets all variables that need to be reset for the next loop
    /// </summary>
    void ClearPlayerState() {
		groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = ConnectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
    }

    /// <summary>
    /// This function returns true if the player meets the peramiters to snap to ground. This prevents the character from floating when going
    /// up slopes.
    /// </summary>
    bool SnapToGround() {
        // Dont snap if we just tried to jump or just came of the ground.
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 6) {
            return false;
        }

        // Dont snap if we are going faster than the snap speed.
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed) { 
            return false;
        }

        // Dont snap if there is no ground to snap too.
        if (!Physics.Raycast(RigidBody.position, Vector3.down, out RaycastHit hit, pc.height * 0.5f + probeDistance, probeMask)) {
            return false;
        }

        // Dont snap if its a slope that is too steep.
        float upDot = Vector3.Dot(Vector3.up, hit.normal);
        if (upDot < minDot) {
            return false;
        }

        // Snap to ground
        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f) {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        connectedBody = hit.rigidbody;
        return true;
    }

    /// <summary>
    /// This function is to make 2 steep coliders on eather side, a ground contact.
    /// </summary>
    bool CheckSteepContacts() {
        if (steepContactCount > 1) {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(Vector3.up, steepNormal);
            if (upDot >= minDot) {
                steepContactCount = 0;
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Update Connection state is for the rigidbody that we are connected too.
    /// </summary>
    void UpdateConnectionState() {
        if (connectedBody == previousConnectedBody) {
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            ConnectionVelocity = connectionMovement / TickDelta;
        }
        connectionWorldPosition = RigidBody.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }
}


public static class PlayerFunctionHelpers{
    public static Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
}
