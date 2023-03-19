public class EscapeUIMenu : UIBaseState {
    public EscapeUIMenu(UIManager ctx, UIStateCashe cashe) : base(ctx, cashe) {}

    public override void OnEscape() {
        ctx.CurrentState = cashe.GetState(UIStates.Game);
    }

    public override UIStates GetState() {
        return UIStates.Escape;
    }
}