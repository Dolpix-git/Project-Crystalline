public class DebugUIStates : UIBaseState {
    public DebugUIStates(UIManager ctx, UIStateCashe cashe) : base(ctx, cashe) {}

    public override void OnEscape() {
        ctx.CurrentState = cashe.GetState(UIStates.Escape);
    }

    public override void OnDebug() {
        ctx.CurrentState = cashe.GetState(UIStates.Game);
    }

    public override void OnLeaderBoardOpen() {
        ctx.CurrentState = cashe.GetState(UIStates.LeaderBoard);
    }

    public override UIStates GetState() {
        return UIStates.Debug;
    }
}