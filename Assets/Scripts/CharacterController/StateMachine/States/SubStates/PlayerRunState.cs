using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerBaseState{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory){}


    public override void EnterState() { }
    public override void UpdateState() {
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
}
