public abstract class UIBaseState {
    protected UIManager ctx;
    protected UIStateCashe cashe;
    public UIBaseState(UIManager ctx, UIStateCashe cashe) {
        this.ctx = ctx;
        this.cashe = cashe;
    }

    public virtual void OnEscape() { }
    public virtual void OnDebug() { }
    public virtual void OnLeaderBoardOpen() { }
    public virtual void OnLeaderBoardClose() { }
    public virtual void OnBuyMenu() { }

    public abstract UIStates GetState();
}
