using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallingState : PlayerBaseState{
    public PlayerFallingState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory){
        IsRootState = true;
    }

    #region States.
    public override void EnterState() {
        InitiatizeSubState();
    }
    public override void UpdateState() {
        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void CheckSwitchStates() {
        // Check if grounded if so then change root to grounded
    }
    public override void InitiatizeSubState() {
        if (Ctx.MoveData.Movement.magnitude != 0 && Ctx.MoveData.Sprint){
            SetSubState(Cashe.Run());
        }else if (Ctx.MoveData.Movement.magnitude != 0){
            SetSubState(Cashe.Walk());
        }else{
            SetSubState(Cashe.Idle());
        }
    }
    public override PlayerStates PlayerState() {
        return PlayerStates.falling;
    }
    #endregion
}
