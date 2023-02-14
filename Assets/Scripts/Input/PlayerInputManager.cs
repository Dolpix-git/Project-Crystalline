using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.iOS;

public class PlayerInputManager : MonoBehaviour{
    public static PlayerInputManager Instance { get; private set; }
    private PlayerInputActions playerInputActions;

    private bool jumpPress;
    private bool sprintHold;
    private bool crouchSlideHold;
    private Vector2 movement;
    private Vector2 look;
    private bool throwPress;

    private Camera cam;


    private bool mouseToggle = false;
    bool isPaused = false;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Sprint.performed += Sprint;
        playerInputActions.Player.CrouchSlide.performed += CrouchSlide;
        playerInputActions.Player.ToggleMouse.performed += ToggleMouse;
        playerInputActions.Player.Throw.performed += Throw;

        cam = Camera.main;
    }

    private void Update() {
        movement = playerInputActions.Player.Movement.ReadValue<Vector2>();
        if (mouseToggle) {
            look = playerInputActions.Player.Look.ReadValue<Vector2>();
        }
    }
    void OnApplicationFocus(bool hasFocus) {
        isPaused = !hasFocus;
    }

    void OnApplicationPause(bool pauseStatus) {
        isPaused = pauseStatus;
    }

    private void Jump(InputAction.CallbackContext context) {
        if (context.performed) {
            jumpPress = true;
        }
    }
    private void Sprint(InputAction.CallbackContext context) {
        if (context.started) {
            sprintHold = true;
        } else if(context.canceled){
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
    private void ToggleMouse(InputAction.CallbackContext context) {
        if (context.performed && !isPaused) {
            mouseToggle = !mouseToggle;

            if (mouseToggle) {
                Cursor.lockState = CursorLockMode.Locked;
            } else {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
    private void Throw(InputAction.CallbackContext context) {
        if (context.performed) {
            throwPress = true;
        }
    }

    public void GetCharacterControllerInputs(out PlayerMoveData md) {
        // might need to add a check for if there is no data to send, to prevent weird behavior (might be patched in new version of fishnet)


        // TOO DO: Hook up to camera function so that cam data is sent
        md = new PlayerMoveData(jumpPress, sprintHold,crouchSlideHold, movement, cam.transform.right, cam.transform.forward); 

        // All press statements must be set to false after use here, to not miss a player input
        jumpPress = false;
    }
    public bool GetWeaponsInput() {

        bool tempBool = throwPress;
        throwPress = false;

        return tempBool;
    }
    public Vector3 GetCamForward() {
        return cam.transform.forward;
    }
    public Vector3 GetCamRight() {
        return cam.transform.right;
    }

    public Vector2 GetMouseVector2() {
        return look;
    }
}
