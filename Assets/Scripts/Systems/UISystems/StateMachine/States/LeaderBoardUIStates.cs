public class LeaderBoardUIStates : UIBaseState {
    public LeaderBoardUIStates(UIManager ctx, UIStateCashe cashe) : base(ctx, cashe) { }

    public override void OnEscape() {
        ctx.CurrentState = cashe.GetState(UIStates.Escape);
    }

    public override void OnDebug() {
        ctx.CurrentState = cashe.GetState(UIStates.Debug);
    }

    public override void OnLeaderBoardClose() {
        ctx.CurrentState = cashe.GetState(UIStates.Game);
    }

    public override void OnBuyMenu() {
        ctx.CurrentState = cashe.GetState(UIStates.BuyMenu);
    }
    public override UIStates GetState() {
        return UIStates.LeaderBoard;
    }
}

