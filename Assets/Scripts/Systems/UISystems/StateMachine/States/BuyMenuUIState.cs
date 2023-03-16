using UnityEngine;

public class BuyMenuUIState : UIBaseState {
    public BuyMenuUIState(UIManager ctx, UIStateCashe cashe) : base(ctx, cashe) {
        UIInputManager.Instance.OnBuyMenu += Instance_OnBuyMenu;
        UIInputManager.Instance.OnLeaderBoard += Instance_OnLeaderBoard;
        UIInputManager.Instance.OnDebug += Instance_OnDebug;
        UIInputManager.Instance.OnEscape += Instance_OnEscape;
    }
    private void Instance_OnEscape() {
        if (cashe.EventTime == Time.time) return;
        if (!(ctx.CurrentState == this)) return;
        cashe.EventTime = Time.time;
        ctx.CurrentState = cashe.GetState(UIStates.Escape);
        ctx.ChangedState();
    }

    private void Instance_OnDebug() {
        if (cashe.EventTime == Time.time) return;
        if (!(ctx.CurrentState == this)) return;
        cashe.EventTime = Time.time;
        ctx.CurrentState = cashe.GetState(UIStates.Debug);
        ctx.ChangedState();
    }

    private void Instance_OnLeaderBoard(bool obj) {
        if (cashe.EventTime == Time.time) return;
        if (!(ctx.CurrentState == this)) return;
        if (!obj) return;
        cashe.EventTime = Time.time;
        ctx.CurrentState = cashe.GetState(UIStates.LeaderBoard);
        ctx.ChangedState();
    }

    private void Instance_OnBuyMenu() {
        if (cashe.EventTime == Time.time) return;
        if (!(ctx.CurrentState == this)) return;
        cashe.EventTime = Time.time;
        ctx.CurrentState = cashe.GetState(UIStates.Game);
        ctx.ChangedState();
    }

    public override UIStates GetState() {
        return UIStates.BuyMenu;
    }
}

