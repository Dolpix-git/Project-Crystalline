using Unity.Mathematics;
using UnityEngine;

public class PlayerRunState : PlayerBaseState{
    #region Private.
    /// <summary>
    /// Velocity acceleration when grounded.
    /// </summary>
    private float groundAcc = 75;
    /// <summary>
    /// Velocity acceleration when not grounded.
    /// </summary>
    private float airAcc = 50;
    /// <summary>
    /// The max velocity of the player.
    /// </summary>
    private float maxSpeed = 7;
    /// <summary>
    /// A clamp on the movement in each direction (backward, forward, left, right)
    /// Used to make going some directions faster than others, for example no running backward.
    /// </summary>
    private float4 speedClamp = new float4(-10, 10, -10, 10);
    #endregion
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory){}

    #region States.
    public override void EnterState() {
        CustomLogger.Log("ENTERED RUNNING STATE");
    }
    public override void UpdateState() {
        AdjustVelocity();

        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void InitiatizeSubState() { }
    public override void CheckSwitchStates() {
        if (Ctx.MoveData.Movement.magnitude != 0 && !Ctx.MoveData.Sprint){
            SwitchState(Cashe.Walk());
        }else if(Ctx.MoveData.Movement.magnitude == 0){
            SwitchState(Cashe.Idle());
        }
    }
    public override PlayerStates PlayerState() {
        return PlayerStates.run;
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

        float acceleration = Ctx.OnGround ? groundAcc : airAcc;
        float maxSpeedChange = acceleration * Ctx.TickDelta;

        Vector3 desiredVelocity = new Vector3(
            Mathf.Clamp(Ctx.MoveData.Movement.x * maxSpeed, speedClamp.x, speedClamp.y),
            0f,
            Mathf.Clamp(Ctx.MoveData.Movement.y * maxSpeed, speedClamp.z, speedClamp.w));

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        Ctx.Velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    #endregion
}
