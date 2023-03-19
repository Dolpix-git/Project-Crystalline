using UnityEngine;

public class UIManager : MonoBehaviour {
    #region Public.
    private static UIManager _instance;
    public static UIManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<UIManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("UIManager"));
                    _instance = obj.AddComponent<UIManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    private UIBaseState currentState;
    private UIStateCashe uiStateCashe;
    private IPanel[] childPanels;

    public bool IsMenu;

    public UIBaseState CurrentState { get => currentState; set { currentState = value; ChangedState(); } }

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
        childPanels = GetComponentsInChildren<IPanel>();

        uiStateCashe = new(this);

        UIInputManager.Instance.OnBuyMenu += Instance_OnBuyMenu;
        UIInputManager.Instance.OnDebug += Instance_OnDebug;
        UIInputManager.Instance.OnEscape += Instance_OnEscape;
        UIInputManager.Instance.OnLeaderBoardOpen += Instance_OnLeaderBoardOpen;
        UIInputManager.Instance.OnLeaderBoardClose += Instance_OnLeaderBoardClose;
    }

    private void Instance_OnLeaderBoardClose() {
        currentState.OnLeaderBoardClose();
    }

    private void Instance_OnLeaderBoardOpen() {
        currentState.OnLeaderBoardOpen();
    }

    private void Instance_OnEscape() {
        currentState.OnEscape();
    }

    private void Instance_OnDebug() {
        currentState.OnDebug();
    }

    private void Instance_OnBuyMenu() {
        currentState.OnBuyMenu();
    }

    private void Start() {
        CurrentState = uiStateCashe.GetState(UIStates.Game);
    }

    public void ChangedState() {
        Log.LogMsg($"Changed UI State to {currentState}");
        TurnOffAllStateUI();
        TurnOnStateUI(currentState.GetState());

        if (currentState.GetState() != UIStates.Game) { 
            IsMenu = true;
            Cursor.lockState = CursorLockMode.None;
        } else { 
            IsMenu = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public void TurnOffAllStateUI() {
        for (int i = 0; i < childPanels.Length; i++) {
            childPanels[i].SetVisible(false);
        }
    }
    public void TurnOnStateUI(UIStates state) {
        for (int i = 0; i < childPanels.Length; i++) {
            if (childPanels[i].HasState(state)) {
                childPanels[i].SetVisible(true);
            }
        }
    }
}
