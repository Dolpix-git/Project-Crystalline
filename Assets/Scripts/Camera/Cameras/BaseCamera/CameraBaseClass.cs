public abstract class CameraBaseClass{
    /// <summary>
    /// Refrence to player camera manager
    /// </summary>
    private PlayerCameraManager playerCameraManager;

    public PlayerCameraManager PlayerCameraManager { get => playerCameraManager; set => playerCameraManager = value; }

    protected CameraBaseClass(PlayerCameraManager playerCameraManager) {
        this.playerCameraManager = playerCameraManager;
    }

    /// <summary>
    /// Camera update called on Update().
    /// </summary>
    public abstract void UpdateCamera();
    /// <summary>
    /// Destroys camera for clean up.
    /// </summary>
    public abstract void DestroyCamera();
}
