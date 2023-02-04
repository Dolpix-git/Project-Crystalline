using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory){}

    public override void EnterState() {
        InitiatizeSubState();
    }
    public override void UpdateState() {
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
}
