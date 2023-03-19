using System.Collections.Generic;

public class UIStateCashe{
    private UIManager context;
    private Dictionary<UIStates, UIBaseState> states = new Dictionary<UIStates, UIBaseState>();

    public UIStateCashe(UIManager currentContext) {
        context = currentContext;
        states[UIStates.Game] = new GameUIState(context, this);
        states[UIStates.BuyMenu] = new BuyMenuUIState(context, this);
        states[UIStates.LeaderBoard] = new LeaderBoardUIStates(context, this);
        states[UIStates.Debug] = new DebugUIStates(context, this);
        states[UIStates.Escape] = new EscapeUIMenu(context, this);
    }
    public UIBaseState GetState(UIStates state) {
        return states[state];
    }
}

public enum UIStates {
    Game,
    BuyMenu,
    LeaderBoard,
    Debug,
    Escape
}

