using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour{
    public static PlayerInputManager Instance { get; private set; }
   

    private PlayerInputActions playerInputActions;

    private bool jumpPress;
    private bool sprintHold;
    private bool crouchSlideHold;
    private Vector2 movement;
    private Vector2 look;
    private bool throwPress;
    private bool plantSpike;
    private bool interactionHold;

    private Camera cam;


    private bool mouseToggle = false;
    bool isPaused = false;

    public Vector2 Movement { get => movement; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Sprint.started += Sprint;
        playerInputActions.Player.Sprint.canceled += Sprint;
        playerInputActions.Player.CrouchSlide.started += CrouchSlide;
        playerInputActions.Player.CrouchSlide.canceled += CrouchSlide;
        playerInputActions.Player.ToggleMouse.performed += ToggleMouse;
        playerInputActions.Player.Throw.performed += Throw;
        playerInputActions.Player.PlantSpike.performed += PlantSpike;
        playerInputActions.Player.Interaction.started += Interaction;
        playerInputActions.Player.Interaction.canceled += Interaction;

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

    private void PlantSpike(InputAction.CallbackContext context) {
        if (context.performed) {
            plantSpike = true;
        }
    }
    private void Interaction(InputAction.CallbackContext context) {
        if (context.started) {
            interactionHold = true;
        } else if (context.canceled) {
            interactionHold = false;
        }
    }
    public void GetCharacterControllerInputs(out PlayerMoveData md) {
        md = new PlayerMoveData(jumpPress, crouchSlideHold, sprintHold, movement, cam.transform.right, cam.transform.forward); 

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
    public bool GetPlantSpike() {
        bool tempBool = plantSpike;
        plantSpike = false;
        return tempBool;
    }
    public bool GetInteraction() {
        return interactionHold;
    }
}
