using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory) {
        IsRootState = true;
    }

    #region States.
    public override void EnterState() {
    InitiatizeSubState();
        HandleJump();
    }
    public override void UpdateState() {
        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void InitiatizeSubState() {
        if (Ctx.MoveData.Movement.magnitude != 0 && Ctx.MoveData.Sprint){
            SetSubState(Cashe.Run());
        }else if (Ctx.MoveData.Movement.magnitude != 0){
            SetSubState(Cashe.Walk());
        }else {
            SetSubState(Cashe.Idle());
        }
    }
    public override void CheckSwitchStates() {
        // TOO DO: Go to falling state
        // TOO DO: Then go to grounded state

        SwitchState(Cashe.Falling());
    }
    public override PlayerStates PlayerState() {
        return PlayerStates.jump;
    }
    #endregion

    #region Methods.
    void HandleJump(){
        Ctx.Velocity += new Vector3(0f, 4f, 0f);
        //Ctx.RigidBody.AddForce(new Vector3(0,10,0),ForceMode.Impulse);
    }
    #endregion
}
