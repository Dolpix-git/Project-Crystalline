using UnityEngine;

public class FirstPersonCamera : CameraBaseClass {
    /// <summary>
    /// The first person transform we keep a refrence too
    /// </summary>
    private Transform firstPersonTransform;
    /// <summary>
    /// 
    /// </summary>
    private Vector2 cameraAngles;
    private void Start() {
        FirstPersonCameraEvent.OnFirstPersonCamera += PlayerCameraFirstPerson;
    }
    public override void DestroyCamera() {
        FirstPersonCameraEvent.OnFirstPersonCamera -= PlayerCameraFirstPerson;
        firstPersonTransform = null;
        Debug.Log("Camera FPS Transform Destroyed!");
    }
    public override void UpdateCamera() {
        if (firstPersonTransform != null) {
            CameraPosition();
            CameraRotation();
        }
    }

    /// <summary>
    /// Updates camera position
    /// </summary>
    private void CameraPosition() {
        PlayerCameraManager.transform.position = firstPersonTransform.position;
    }

    /// <summary>
    /// Updates camera rotation
    /// </summary>
    private void CameraRotation() {
        cameraAngles.x += Mathf.Clamp(-PlayerInputManager.Instance.GetMouseVector2().y * PlayerCameraManager.CameraRotationSpeed * Time.unscaledDeltaTime, -PlayerCameraManager.MaxCameraDelta, PlayerCameraManager.MaxCameraDelta);
        cameraAngles.y += Mathf.Clamp(PlayerInputManager.Instance.GetMouseVector2().x * PlayerCameraManager.CameraRotationSpeed * Time.unscaledDeltaTime, -PlayerCameraManager.MaxCameraDelta, PlayerCameraManager.MaxCameraDelta);

        cameraAngles.x = Mathf.Clamp(cameraAngles.x % 180, -PlayerCameraManager.MaxCameraAngle, PlayerCameraManager.MaxCameraAngle);

        PlayerCameraManager.transform.rotation = Quaternion.Euler(cameraAngles);
        PlayerCameraManager.transform.rotation = Quaternion.LookRotation(PlayerCameraManager.transform.forward, Vector3.up);
    }

    /// <summary>
    /// Set the first person camera transform of the first person camera class.
    /// </summary>
    /// <param name="obj">Refrence to the players head transform</param>
    private void PlayerCameraFirstPerson(Transform obj) {
        firstPersonTransform = obj;
    }
}
