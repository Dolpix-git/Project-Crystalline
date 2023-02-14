using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory){}

    private float groundAcc = 90;
    private float airAcc = 50;
    private float maxSpeed = 0;
    private float4 speedClamp = new float4(-10, 10, -10, 10);

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

    public override PlayerStates PlayerState() {
        return PlayerStates.idle;
    }
}