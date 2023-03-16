public abstract class UIBaseState {
    protected UIManager ctx;
    protected UIStateCashe cashe;
    public UIBaseState(UIManager ctx, UIStateCashe cashe) {
        this.ctx = ctx;
        this.cashe = cashe;
    }

    public abstract UIStates GetState();
}
