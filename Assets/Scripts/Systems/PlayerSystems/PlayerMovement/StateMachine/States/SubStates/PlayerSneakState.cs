using UnityEngine;

public class PlayerSneakState : PlayerBaseState{
    public PlayerSneakState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory){}

    #region States.
    public override void EnterState() {}
    public override void UpdateState() {
        GDSSReturn();
        GDSS();

        AdjustVelocity();

        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void InitiatizeSubState() { }
    public override void CheckSwitchStates() {
        if (Ctx.MoveData.Crouch) {
            SwitchState(Cashe.Crouching());
        } else if (Ctx.MoveData.Movement.magnitude != 0 && !Ctx.MoveData.Sneak){
            SwitchState(Cashe.Walk());
        } else if(Ctx.MoveData.Movement.magnitude == 0){
            SwitchState(Cashe.Idle());
        } 
    }
    public override PlayerStates PlayerState() {
        return PlayerStates.sneak;
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

        float acceleration = Ctx.OnGround ? Ctx.PlayerEffects.AirAcc(Ctx.Velocity.magnitude) : Ctx.PlayerEffects.AirAcc(Ctx.Velocity.magnitude);
        float maxSpeedChange = acceleration * Ctx.TickDelta;

        Vector3 desiredVelocity = new Vector3(
            Mathf.Clamp(Ctx.MoveData.Movement.x * Ctx.PlayerEffects.SneakMaxSpeed , Ctx.PlayerEffects.SneakSpeedClamp.x, Ctx.PlayerEffects.SneakSpeedClamp.y),
            0f,
            Mathf.Clamp(Ctx.MoveData.Movement.y * Ctx.PlayerEffects.SneakMaxSpeed, Ctx.PlayerEffects.SneakSpeedClamp.z, Ctx.PlayerEffects.SneakSpeedClamp.w));

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        Ctx.Velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    #endregion

    #region GDSS.
    void GDSS() {
        RaycastHit ray;
        Vector3 proJect = (Ctx.RigidBody.velocity + Ctx.ForwardAxis).normalized;
        proJect.y = 0;
        Vector3 playerProjected = Ctx.RigidBody.transform.position + (proJect * 0.15f) + Ctx.PlayerCollider.center;
        if (Physics.SphereCast(playerProjected, 0.195f, Vector3.down, out ray, Ctx.OriginalPlayerHeight * 0.5f - 0.3f, Ctx.PlayerEffects.ProbeMask)) {
            ChangeColliderHeight(Ctx.OriginalPlayerHeight * 0.5f - (ray.distance + 0.1f));
        }
    }
    void ChangeColliderHeight(float a) {
        Vector3 playerVel = Ctx.Velocity;
        playerVel.y = Mathf.Max(4 * a, playerVel.y);
        Ctx.Velocity = playerVel;

        Ctx.PlayerCollider.height = Ctx.OriginalPlayerHeight - a;
        Ctx.PlayerCollider.center = new Vector3(0, a * 0.5f, 0);
    }
    void GDSSReturn() {
        if (Ctx.PlayerCollider.height < Ctx.OriginalPlayerHeight) {
            Ctx.PlayerCollider.height = Mathf.Min(Ctx.OriginalPlayerHeight, Ctx.PlayerCollider.height + Ctx.TickDelta * 5f);
            Ctx.PlayerCollider.center = new Vector3(0, (Ctx.OriginalPlayerHeight - Ctx.PlayerCollider.height) * 0.5f, 0);
        }
    }
    #endregion
}
