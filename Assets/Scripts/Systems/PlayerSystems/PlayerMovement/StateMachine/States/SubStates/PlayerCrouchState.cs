using UnityEngine;

public class PlayerCrouchState : PlayerBaseState {
    public PlayerCrouchState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory) { }

    #region States.
    public override void EnterState() {
        Debug.Log("EnterCrouchState");
        Debug.Log(Ctx.OnGround);
        if (Ctx.OnGround) {
            Ctx.Velocity += Vector3.down * 5f;
        }
    }
    public override void UpdateState() {
        if (!Ctx.MoveData.Crouch) {
            UnCrouch();
        } else {
            ChangeColliderHeight(Ctx.PlayerEffects.CrouchDelta);
            //AkSoundEngine.PostEvent("Crouch", Ctx.RigidBody.gameObject);
        }

        AdjustVelocity();

        CheckSwitchStates();
    }
    public override void ExitState() {
        Debug.Log("EXIT CROUCH");
    }
    public override void InitiatizeSubState() { }
    public override void CheckSwitchStates() {
        if (Ctx.MoveData.Crouch) {
            if (Ctx.Velocity.magnitude >= Ctx.PlayerEffects.SlidingActivationSpeed) {
                SwitchState(Cashe.Sliding());
            }
        } else if (Ctx.PlayerCollider.height >= Ctx.OriginalPlayerHeight) {
            if (Ctx.MoveData.Movement.magnitude != 0 && Ctx.MoveData.Sprint) {
                SwitchState(Cashe.Run());
            } else if (Ctx.MoveData.Movement.magnitude != 0) {
                SwitchState(Cashe.Walk());
            } else if (Ctx.MoveData.Movement.magnitude == 0) {
                SwitchState(Cashe.Idle());
            }
        }
    }
    public override PlayerStates PlayerState() {
        return PlayerStates.crouching;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Handles getting the players new velocity based on what its standing on, the players input, and acceleration values.
    /// </summary>
    void AdjustVelocity() {
        Vector3 xAxis = PlayerFunctionHelpers.ProjectDirectionOnPlane(Ctx.RightAxis, Ctx.ContactNormal);
        Vector3 zAxis = PlayerFunctionHelpers.ProjectDirectionOnPlane(Ctx.ForwardAxis, Ctx.ContactNormal);

        Vector3 relativeVelocity = Ctx.Velocity - Ctx.ConnectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        float acceleration = Ctx.OnGround ? Ctx.PlayerEffects.CrouchGroundAcc : Ctx.PlayerEffects.CrouchAirAcc;
        float maxSpeedChange = acceleration * Ctx.TickDelta;

        Vector3 desiredVelocity = new Vector3(
            Mathf.Clamp(Ctx.MoveData.Movement.x * Ctx.PlayerEffects.CrouchMaxSpeed, Ctx.PlayerEffects.CrouchSpeedClamp.x, Ctx.PlayerEffects.CrouchSpeedClamp.y),
            0f,
            Mathf.Clamp(Ctx.MoveData.Movement.y * Ctx.PlayerEffects.CrouchMaxSpeed, Ctx.PlayerEffects.CrouchSpeedClamp.z, Ctx.PlayerEffects.CrouchSpeedClamp.w));

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        Ctx.Velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void ChangeColliderHeight(float a) {
        Ctx.PlayerCollider.height = Ctx.OriginalPlayerHeight - a;
        Ctx.PlayerCollider.center = new Vector3(0, a * 0.5f, 0);
    }

    void UnCrouch() {
        RaycastHit hit;

        Vector3 bottom = Ctx.RigidBody.position + (Ctx.PlayerCollider.center) + (Ctx.PlayerCollider.height * 0.5f * Vector3.down) + (Ctx.PlayerCollider.radius * Vector3.up);

        if (Physics.SphereCast(bottom, Ctx.PlayerCollider.radius - 0.01f, Vector3.up, out hit, Ctx.OriginalPlayerHeight)) {
            ChangeColliderHeight(Mathf.Min(Ctx.OriginalPlayerHeight - (hit.distance + Ctx.PlayerCollider.radius), Ctx.PlayerEffects.CrouchDelta));
        } else {
            GDSSReturn();
        }
    }
    void GDSSReturn() {
        if (Ctx.PlayerCollider.height < Ctx.OriginalPlayerHeight) {
            Ctx.PlayerCollider.height = Mathf.Min(Ctx.OriginalPlayerHeight, Ctx.PlayerCollider.height + Ctx.TickDelta * 5f);
            Ctx.PlayerCollider.center = new Vector3(0, (Ctx.OriginalPlayerHeight - Ctx.PlayerCollider.height) * 0.5f, 0);
        }
    }
    #endregion
}
