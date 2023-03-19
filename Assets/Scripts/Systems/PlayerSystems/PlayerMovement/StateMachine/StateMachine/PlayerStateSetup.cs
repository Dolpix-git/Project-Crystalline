using UnityEngine;

public class PlayerStateSetup{
    #region Private.
    /// <summary>
    /// Refrence to the statemachine that owns this script
    /// </summary>
    private PlayerStateMachine playerStateMachine;
    #endregion

    #region Player Variables.
    public int groundContactCount, steepContactCount; // How many colliders are we touching and below the maxGroundAngle
    private Rigidbody connectedBody, previousConnectedBody; // all the rigidbodys of what we are colliding with. (provided with NULL if there isnt on)
    public Vector3 contactNormal, steepNormal; // The combination of all the normals that are considered ground or steep
    public Vector3 connectionVelocity; // Velocitys for applying to our rigidbody, this is for what we are standing on.
    private Vector3 connectionWorldPosition, connectionLocalPosition; // Vector3 for possitions for standing on moving objects.

    public Vector3 rightAxis, forwardAxis;

    private float minDot;

    public int stepsSinceLastGrounded, stepsSinceLastJump; // Used to count the amount of physics steps we are not grounded for

    public float MinDot { get => minDot; }
    #endregion


    public PlayerStateSetup(PlayerStateMachine playerStateMachine) {
        this.playerStateMachine = playerStateMachine;

        minDot = Mathf.Cos(playerStateMachine.PlayerEffects.MaxSlope * Mathf.Deg2Rad);
    }

    public void Update() {
        UpdatePlayerState();
        PlayerInputs();
    }

    #region Methods.
    /// <summary>
    /// Take the user inputs and makes them relative to the character. also handles vector cheating.
    /// </summary>
    private void PlayerInputs() {
        // This is to prevent clients from cheating, as they could in theory alter the vector2 on their machine
        playerStateMachine.SetPlayerMovementData(Vector2.ClampMagnitude(playerStateMachine.MoveData.Movement, 1f));

        rightAxis = PlayerFunctionHelpers.ProjectDirectionOnPlane(playerStateMachine.MoveData.CameraRight, Vector3.up);
        forwardAxis = PlayerFunctionHelpers.ProjectDirectionOnPlane(playerStateMachine.MoveData.CameraForward, Vector3.up);
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
    private void UpdatePlayerState() {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        // Sets the velocity of RB, normalises the contact normals, resets the jump counts
        playerStateMachine.Velocity = playerStateMachine.RigidBody.velocity;
        if (playerStateMachine.OnGround || SnapToGround() || CheckSteepContacts()) {
            stepsSinceLastGrounded = 0;
            if (groundContactCount > 1) {
                contactNormal.Normalize();
            }
        }
        // If were in the air we would want to jump straight up not the direcion of the surface we were previously on
        else {
            contactNormal = Vector3.up;
        }
        if (connectedBody) {
            if (connectedBody.isKinematic || connectedBody.mass >= playerStateMachine.RigidBody.mass) {
                UpdateConnectionState();
            }
        }
    }

    /// <summary>
    /// clear state resets all variables that need to be reset for the next loop
    /// </summary>
    public void ClearPlayerState() {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = connectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
    }

    /// <summary>
    /// This function returns true if the player meets the peramiters to snap to ground. This prevents the character from floating when going
    /// up slopes.
    /// </summary>
    private bool SnapToGround() {
        // Dont snap if we just tried to jump or just came of the ground.
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 6) {
            return false;
        }

        // Dont snap if we are going faster than the snap speed.
        float speed = playerStateMachine.Velocity.magnitude;
        if (speed > playerStateMachine.PlayerEffects.MaxSnapSpeed) {
            return false;
        }

        // Dont snap if there is no ground to snap too.
        if (!Physics.Raycast(playerStateMachine.RigidBody.position, Vector3.down, out RaycastHit hit, playerStateMachine.PlayerCollider.height * 0.5f + playerStateMachine.PlayerEffects.ProbeDistance, playerStateMachine.PlayerEffects.ProbeMask)) {
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
        float dot = Vector3.Dot(playerStateMachine.Velocity, hit.normal);
        if (dot > 0f) {
            playerStateMachine.Velocity = (playerStateMachine.Velocity - hit.normal * dot).normalized * speed;
        }
        connectedBody = hit.rigidbody;

        return true;
    }

    /// <summary>
    /// This function is to make 2 steep coliders on eather side, a ground contact.
    /// </summary>
    private bool CheckSteepContacts() {
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
    private void UpdateConnectionState() {
        if (connectedBody == previousConnectedBody) {
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / playerStateMachine.TickDelta;
        }
        connectionWorldPosition = playerStateMachine.RigidBody.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }


    #endregion
}
