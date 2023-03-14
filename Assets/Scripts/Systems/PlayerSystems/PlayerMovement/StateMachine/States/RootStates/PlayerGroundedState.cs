using UnityEngine;

public class PlayerGroundedState : PlayerBaseState{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory) {
        IsRootState = true;
    }

    #region States.
    public override void EnterState() {
        InitiatizeSubState();
    }
    public override void UpdateState(){
        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void InitiatizeSubState() {
        if (Ctx.MoveData.Movement.magnitude != 0 && Ctx.MoveData.Sprint) {
            SetSubState(Cashe.Run());
        } else if (Ctx.MoveData.Movement.magnitude != 0) {
            SetSubState(Cashe.Walk());
        } else {
            SetSubState(Cashe.Idle());
        }
    }
    public override void CheckSwitchStates(){
        if (Ctx.MoveData.Jump) {
            SwitchState(Cashe.Jump());
        }else if (!Ctx.OnGround) {
            SwitchState(Cashe.Falling());
           
        }
    }
    public override PlayerStates PlayerState() {
        return PlayerStates.grounded;
    }
    #endregion
}
