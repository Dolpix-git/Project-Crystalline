using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputManager : MonoBehaviour {
    #region Public.
    private static UIInputManager _instance;
    public static UIInputManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<UIInputManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("UIInputManager"));
                    _instance = obj.AddComponent<UIInputManager>();
                }
            }
            return _instance;
        }
    }

    #endregion

    #region Private.
    private PlayerInputActions playerInputActions;
    #endregion

    #region Getters Setters.
    #endregion

    #region Events.
    public event Action OnEscape;
    public event Action OnBuyMenu;
    public event Action OnLeaderBoardOpen;
    public event Action OnLeaderBoardClose;
    public event Action OnDebug;
    #endregion


    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }

        playerInputActions = new PlayerInputActions();
        playerInputActions.UI.Enable();

        playerInputActions.UI.Escape.performed += Escape_performed;

        playerInputActions.UI.BuyMenu.performed += BuyMenu_performed;

        playerInputActions.UI.LeaderBoard.started += LeaderBoard_started;
        playerInputActions.UI.LeaderBoard.canceled += LeaderBoard_canceled;

        playerInputActions.UI.Debug.performed += Debug_performed;
    }

    private void Escape_performed(InputAction.CallbackContext obj) {
        OnEscape?.Invoke();
    }
    private void BuyMenu_performed(InputAction.CallbackContext obj) {
        OnBuyMenu?.Invoke();
    }
    private void LeaderBoard_started(InputAction.CallbackContext obj) {
        OnLeaderBoardOpen?.Invoke();
    }
    private void LeaderBoard_canceled(InputAction.CallbackContext obj) {
        OnLeaderBoardClose?.Invoke();
    }
    private void Debug_performed(InputAction.CallbackContext obj) {
        OnDebug?.Invoke();
    }
}
