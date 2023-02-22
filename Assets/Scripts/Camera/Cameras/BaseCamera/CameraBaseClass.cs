using FishNet.Object;

public abstract class CameraBaseClass : NetworkBehaviour{
    /// <summary>
    /// Refrence to player camera manager
    /// </summary>
    private PlayerCameraManager playerCameraManager;

    public PlayerCameraManager PlayerCameraManager { get => playerCameraManager; set => playerCameraManager = value; }

    /// <summary>
    /// Camera update called on Update().
    /// </summary>
    public abstract void UpdateCamera();
    /// <summary>
    /// Destroys camera for clean up.
    /// </summary>
    public abstract void DestroyCamera();
}
