using UnityEngine;

public class FirstPersonCamera : CameraBaseClass {
    private Transform firstPersonTransform;
    private Vector2 cameraAngles;
    public FirstPersonCamera(PlayerCameraManager playerCameraManager) : base(playerCameraManager) {
        FirstPersonCameraEvent.OnFirstPersonCamera += PlayerCameraFirstPerson;
    }
    public override void DestroyCamera() {
        FirstPersonCameraEvent.OnFirstPersonCamera -= PlayerCameraFirstPerson;
    }
    public override void UpdateCamera() {
        if (firstPersonTransform is not null) {
            CameraPosition();
            CameraRotation();
        }
    }

    private void CameraPosition() {
        PlayerCameraManager.transform.position = firstPersonTransform.position;
    }

    private void CameraRotation() {
        cameraAngles.x += Mathf.Clamp(-PlayerInputManager.Instance.GetMouseVector2().y * PlayerCameraManager.CameraRotationSpeed * Time.unscaledDeltaTime, -PlayerCameraManager.MaxCameraDelta, PlayerCameraManager.MaxCameraDelta);
        cameraAngles.y += Mathf.Clamp(PlayerInputManager.Instance.GetMouseVector2().x * PlayerCameraManager.CameraRotationSpeed * Time.unscaledDeltaTime, -PlayerCameraManager.MaxCameraDelta, PlayerCameraManager.MaxCameraDelta);

        cameraAngles.x = Mathf.Clamp(cameraAngles.x % 180, -PlayerCameraManager.MaxCameraAngle, PlayerCameraManager.MaxCameraAngle);

        PlayerCameraManager.transform.rotation = Quaternion.Euler(cameraAngles);
        PlayerCameraManager.transform.rotation = Quaternion.LookRotation(PlayerCameraManager.transform.forward, Vector3.up);
    }

    private void PlayerCameraFirstPerson(Transform obj) {
        firstPersonTransform = obj;
    }
}
