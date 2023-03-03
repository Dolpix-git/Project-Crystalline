using UnityEngine;

public class SpectatorCamera : CameraBaseClass {
    private Vector2 cameraAngles;
    private float speed = 7;
    public override void SetCamera() { }
    public override void DestroyCamera() {}
    public override void UpdateCamera() {
        CameraPosition();
        CameraRotation();
    }

    /// <summary>
    /// Updates camera position
    /// </summary>
    private void CameraPosition() {
        Vector3 xAxis = PlayerFunctionHelpers.ProjectDirectionOnPlane(PlayerCameraManager.transform.right, PlayerCameraManager.transform.up);
        Vector3 zAxis = PlayerFunctionHelpers.ProjectDirectionOnPlane(PlayerCameraManager.transform.forward, PlayerCameraManager.transform.up);

        Vector3 desiredVelocity = new Vector3(PlayerInputManager.Instance.Movement.x * speed,0f,PlayerInputManager.Instance.Movement.y * speed) * Time.deltaTime;

        PlayerCameraManager.transform.position += desiredVelocity.x * xAxis + desiredVelocity.z * zAxis;
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
}
