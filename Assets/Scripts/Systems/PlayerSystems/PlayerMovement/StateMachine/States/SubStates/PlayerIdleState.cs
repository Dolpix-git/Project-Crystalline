using UnityEngine;

public class PlayerIdleState : PlayerBaseState{
    #region Private.
    /// <summary>
    /// Velocity acceleration when grounded.
    /// </summary>
    private float groundAcc = 90;
    /// <summary>
    /// Velocity acceleration when not grounded.
    /// </summary>
    private float airAcc = 50;
    #endregion
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory){}

    #region States.
    public override void EnterState() { }
    public override void UpdateState() {
        AdjustVelocity();

        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void InitiatizeSubState() { }
    public override void CheckSwitchStates() {
        if (Ctx.MoveData.Movement.magnitude != 0 && Ctx.MoveData.Sprint){
            SwitchState(Cashe.Run());
        }else if (Ctx.MoveData.Movement.magnitude != 0){
            SwitchState(Cashe.Walk());
        }
    }
    public override PlayerStates PlayerState() {
        return PlayerStates.idle;
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

        Vector3 desiredVelocity = new Vector3(0, 0, 0);

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        Ctx.Velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    #endregion
}
