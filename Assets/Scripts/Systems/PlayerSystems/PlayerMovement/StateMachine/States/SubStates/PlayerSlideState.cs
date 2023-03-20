using UnityEngine;

public class PlayerSlideState : PlayerBaseState {
    public PlayerSlideState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory) : base(currentContext, playerStateFactory) { }

    #region States.
    public override void EnterState() {
        ChangeColliderHeight(Ctx.PlayerEffects.CrouchDelta);
        if (Ctx.OnGround) {
            Ctx.Velocity += Vector3.down * 3.5f;
        }
    }
    public override void UpdateState() {
        if (!Ctx.MoveData.Crouch) {
            UnCrouch();
        } else {
            ChangeColliderHeight(Ctx.PlayerEffects.CrouchDelta);
        }

        if (Ctx.OnGround) {
            Ctx.Velocity *= Ctx.PlayerEffects.SlidingFriction;
        }
        

        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void InitiatizeSubState() { }
    public override void CheckSwitchStates() {
        if (Ctx.Velocity.magnitude < Ctx.PlayerEffects.SlidingDeactivationSpeed || !Ctx.MoveData.Crouch) {
            SwitchState(Cashe.Crouching());
        } else if (Ctx.PlayerCollider.height >= Ctx.OriginalPlayerHeight) {
            if (Ctx.MoveData.Movement.magnitude != 0 && Ctx.MoveData.Sprint) {
                SwitchState(Cashe.Run());
            } else if (Ctx.MoveData.Movement.magnitude != 0) {
                SwitchState(Cashe.Walk());
            } else if (Ctx.MoveData.Movement.magnitude == 0) {
                SwitchState(Cashe.Idle());
            }
        }
    }
    public override PlayerStates PlayerState() {
        return PlayerStates.sliding;
    }
    #endregion

    #region Methods.
    void ChangeColliderHeight(float a) {
        Ctx.PlayerCollider.height = Ctx.OriginalPlayerHeight - a;
        Ctx.PlayerCollider.center = new Vector3(0, a * 0.5f, 0);
    }


    void UnCrouch() {
        RaycastHit hit;

        Vector3 bottom = Ctx.RigidBody.position + (Ctx.PlayerCollider.center) + (Ctx.PlayerCollider.height * 0.5f * Vector3.down) + (Ctx.PlayerCollider.radius * Vector3.up);

        if (Physics.SphereCast(bottom, Ctx.PlayerCollider.radius - 0.01f, Vector3.up, out hit, Ctx.OriginalPlayerHeight)) {
            ChangeColliderHeight(Mathf.Min(Ctx.OriginalPlayerHeight - (hit.distance + Ctx.PlayerCollider.radius), Ctx.PlayerEffects.CrouchDelta));
        } else {
            GDSSReturn();
        }
    }
    void GDSSReturn() {
        if (Ctx.PlayerCollider.height < Ctx.OriginalPlayerHeight) {
            Ctx.PlayerCollider.height = Mathf.Min(Ctx.OriginalPlayerHeight, Ctx.PlayerCollider.height + Ctx.TickDelta * 5f);
            Ctx.PlayerCollider.center = new Vector3(0, (Ctx.OriginalPlayerHeight - Ctx.PlayerCollider.height) * 0.5f, 0);
        }
    }
    #endregion
}
