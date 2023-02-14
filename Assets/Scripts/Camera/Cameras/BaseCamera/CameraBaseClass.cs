public abstract class CameraBaseClass{
    private PlayerCameraManager playerCameraManager;

    public PlayerCameraManager PlayerCameraManager { get => playerCameraManager; set => playerCameraManager = value; }

    protected CameraBaseClass(PlayerCameraManager playerCameraManager) {
        this.playerCameraManager = playerCameraManager;
    }


    public abstract void UpdateCamera();
    public abstract void DestroyCamera();
}
