public class BuyMenuUIState : UIBaseState {
    public BuyMenuUIState(UIManager ctx, UIStateCashe cashe) : base(ctx, cashe) {}
    public override void OnEscape() {
        ctx.CurrentState = cashe.GetState(UIStates.Escape);
    }

    public override void OnDebug() {
        ctx.CurrentState = cashe.GetState(UIStates.Debug);
    }

    public override void OnLeaderBoardOpen() {
        ctx.CurrentState = cashe.GetState(UIStates.LeaderBoard);
    }

    public override void OnBuyMenu() {
        ctx.CurrentState = cashe.GetState(UIStates.Game);
    }

    public override UIStates GetState() {
        return UIStates.BuyMenu;
    }
}

