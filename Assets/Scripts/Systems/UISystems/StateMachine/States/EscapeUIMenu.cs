using UnityEngine;

public class EscapeUIMenu : UIBaseState {
    public EscapeUIMenu(UIManager ctx, UIStateCashe cashe) : base(ctx, cashe) {
        UIInputManager.Instance.OnEscape += Instance_OnEscape;
    }

    private void Instance_OnEscape() {
        if (cashe.EventTime == Time.time) return;
        if (!(ctx.CurrentState == this)) return;
        cashe.EventTime = Time.time;
        ctx.CurrentState = cashe.GetState(UIStates.Game);
        ctx.ChangedState();
    }
    public override UIStates GetState() {
        return UIStates.Escape;
    }
}