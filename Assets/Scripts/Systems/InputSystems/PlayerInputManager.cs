using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour{
    #region Public.
    public static PlayerInputManager Instance { get; private set; }
    #endregion

    #region Private.
    private PlayerInputActions playerInputActions;

    private bool jumpPress;
    private bool sprintHold;
    private bool crouchSlideHold;
    private Vector2 movement;
    private Vector2 look;

    private Camera cam;
    private bool isPaused = false;
    private bool isLocked = false;
    #endregion

    #region Getters Setters.
    public Vector3 CamForward { get => cam.transform.forward; }
    public Vector3 CamRight { get => cam.transform.right; }
    public Vector2 Movement { get => movement; }
    public Vector2 Look { get => look; }
    #endregion

    #region Events.
    public event Action OnThrow;
    public event Action OnDrop;
    public event Action<bool> OnIntercationChange;

    public event Action<int> OnHotbarChange;
    public event Action<int> OnHotbarDelta;
    #endregion


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.UI.Enable();

        playerInputActions.Player.Jump.performed += Jump;

        playerInputActions.Player.Sprint.started += Sprint;
        playerInputActions.Player.Sprint.canceled += Sprint;

        playerInputActions.Player.CrouchSlide.started += CrouchSlide;
        playerInputActions.Player.CrouchSlide.canceled += CrouchSlide;

        playerInputActions.Player.Throw.performed += Throw;

        playerInputActions.Player.Drop.performed += Drop_performed;

        playerInputActions.Player.Interaction.started += Interaction;
        playerInputActions.Player.Interaction.canceled += Interaction;

        playerInputActions.Player.Hotbar1.performed += Hotbar1_performed;
        playerInputActions.Player.Hotbar2.performed += Hotbar2_performed;
        playerInputActions.Player.Hotbar3.performed += Hotbar3_performed;

        playerInputActions.UI.Escape.performed += Escape_performed;

        cam = Camera.main;
    }

    private void Update() {
        HandleScroll();
        movement = playerInputActions.Player.Movement.ReadValue<Vector2>();
        look = playerInputActions.Player.Look.ReadValue<Vector2>();
    }

    void OnApplicationFocus(bool hasFocus) {
        isPaused = !hasFocus;
    }
    void OnApplicationPause(bool pauseStatus) {
        isPaused = pauseStatus;
    }
    private void Escape_performed(InputAction.CallbackContext obj) {
        Debug.Log(isPaused);
        if (isPaused) return;
        isLocked = !isLocked;
        Debug.Log(isLocked);
        if (isLocked) {
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            Cursor.lockState = CursorLockMode.None;
        }
    }


    #region Player Controller Inputs.
    private void Jump(InputAction.CallbackContext context) {
        jumpPress = true;
    }
    private void Sprint(InputAction.CallbackContext context) {
        if (context.started) {
            sprintHold = true;
        } else if (context.canceled) {
            sprintHold = false;
        }
    }
    private void CrouchSlide(InputAction.CallbackContext context) {
        if (context.started) {
            crouchSlideHold = true;
        } else if (context.canceled) {
            crouchSlideHold = false;
        }
    }
    public void GetCharacterControllerInputs(out PlayerMoveData md) {
        md = new PlayerMoveData(jumpPress, crouchSlideHold, sprintHold, movement, cam.transform.right, cam.transform.forward);

        // All press statements must be set to false after use here, to not miss a player input
        jumpPress = false;
    }
    #endregion

    #region Player Weapons and Interactions.
    private void Throw(InputAction.CallbackContext context) {
        OnThrow?.Invoke();
    }
    private void Drop_performed(InputAction.CallbackContext obj) {
        OnDrop?.Invoke();
    }

    private void Interaction(InputAction.CallbackContext context) {
        if (context.started) {
            OnIntercationChange?.Invoke(true);
        } else if (context.canceled) {
            OnIntercationChange?.Invoke(false);
        }
    }
    #endregion

    #region Player Hotbar.
    private void Hotbar3_performed(InputAction.CallbackContext obj) {
        OnHotbarChange?.Invoke(2);
    }

    private void Hotbar2_performed(InputAction.CallbackContext obj) {
        OnHotbarChange?.Invoke(1);
    }

    private void Hotbar1_performed(InputAction.CallbackContext obj) {
        OnHotbarChange?.Invoke(0);
    }
    private void HandleScroll() {
        float z = playerInputActions.Player.HotBarScroll.ReadValue<float>();
        if (z > 0)
            OnHotbarDelta?.Invoke(1);
        else if (z < 0)
            OnHotbarDelta?.Invoke(-1);
    }
    #endregion
}
