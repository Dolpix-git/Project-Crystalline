using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory){}

    public override void EnterState() { }
    public override void UpdateState(){
        Movement();
        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void InitiatizeSubState() { }
    public override void CheckSwitchStates() {
        if (Ctx.MoveData.Movement.magnitude != 0 && Ctx.MoveData.Sprint){
            SwitchState(Cashe.Run());
        }else if(Ctx.MoveData.Movement.magnitude == 0){
            SwitchState(Cashe.Idle());
        }
    }



    private void Movement(){
        Vector3 forces = new Vector3(Ctx.MoveData.Movement.x, 0, Ctx.MoveData.Movement.y) * 5;
        Ctx.Velocity += forces;
    }
}
