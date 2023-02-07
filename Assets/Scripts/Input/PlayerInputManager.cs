using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.iOS;

public class PlayerInputManager : MonoBehaviour{
    public static PlayerInputManager Instance { get; private set; }
    private PlayerInputActions playerInputActions;

    private bool jumpPress;
    private bool sprintHold;
    private bool crouchSlideHold;
    private Vector2 movement;
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
    }

    private void Update() {
        movement = playerInputActions.Player.Movement.ReadValue<Vector2>();
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

    public void GetCharacterControllerInputs(out PlayerMoveData md) {
        // might need to add a check for if there is no data to send, to prevent weird behavior (might be patched in new version of fishnet)


        // TOO DO: Hook up to camera function so that cam data is sent
        md = new PlayerMoveData(jumpPress, sprintHold,crouchSlideHold, movement, Vector3.right,Vector3.forward); 

        // All press statements must be set to false after use here, to not miss a player input
        jumpPress = false;
    }
}
