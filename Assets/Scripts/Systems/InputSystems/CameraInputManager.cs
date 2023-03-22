using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputManager : MonoBehaviour{
    #region Public.
    private static CameraInputManager _instance;
    public static CameraInputManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<CameraInputManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("CameraInputManager"));
                    _instance = obj.AddComponent<CameraInputManager>();
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
    public event Action OnNextCam;
    public event Action OnPrevCam;
    #endregion


    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }

        playerInputActions = new PlayerInputActions();
        playerInputActions.Camera.Enable();

        playerInputActions.Camera.NextCamera.performed += NextCamera_performed;
        playerInputActions.Camera.PreviousCamera.performed += PreviousCamera_performed;
    }


    private void NextCamera_performed(InputAction.CallbackContext obj) {
        OnNextCam?.Invoke();
    }
    private void PreviousCamera_performed(InputAction.CallbackContext obj) {
        OnPrevCam?.Invoke();
    }
}
