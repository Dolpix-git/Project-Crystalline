using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraTypes{
    locked,
    firstPerson,
    teamSpectator,
    Spectator
}
public class PlayerCameraManager : MonoBehaviour{
    [SerializeField] private float cameraRotationSpeed;
    [SerializeField] private float maxCameraDelta, maxCameraAngle;

    private Dictionary<CameraTypes, CameraBaseClass> cameraClasses = new Dictionary<CameraTypes, CameraBaseClass>();
    private CameraTypes currentCamera = CameraTypes.Spectator;

    
    public float CameraRotationSpeed { get => cameraRotationSpeed; set => cameraRotationSpeed = value; }
    public float MaxCameraDelta { get => maxCameraDelta; set => maxCameraDelta = value; }
    public float MaxCameraAngle { get => maxCameraAngle; set => maxCameraAngle = value; }

    private void Awake() {
        CameraBaseClass camera = gameObject.AddComponent<LockedCamera>();
        camera.PlayerCameraManager = this;
        cameraClasses.Add(CameraTypes.locked, camera);

        camera = gameObject.AddComponent<FirstPersonCamera>();
        camera.PlayerCameraManager = this;
        cameraClasses.Add(CameraTypes.firstPerson, camera);

        camera = gameObject.AddComponent<TeamSpectatorCamera>();
        camera.PlayerCameraManager = this;
        cameraClasses.Add(CameraTypes.teamSpectator, camera);

        camera = gameObject.AddComponent<SpectatorCamera>();
        camera.PlayerCameraManager = this;
        cameraClasses.Add(CameraTypes.Spectator, camera);


        PlayerManager.Instance.OnSpawned += Instance_OnSpawned;
    }

    private void Instance_OnSpawned(FishNet.Object.NetworkObject obj) {
        obj.GetComponent<Health>().OnDeath += Health_OnDeath;
        obj.GetComponent<Health>().OnRespawned += Health_OnRespawned;
        SetSpectator();
    }

    private void OnDestroy() {
        PlayerManager.Instance.OnSpawned -= Instance_OnSpawned;
        foreach (CameraBaseClass camera in cameraClasses.Values) {
            camera.DestroyCamera();
        }   
    }

    private void LateUpdate() {
        cameraClasses[currentCamera].UpdateCamera();
    }

    private void Health_OnRespawned() {
        CustomLogger.Log(LogCategories.Camera, "Switched to First person");
        currentCamera = CameraTypes.firstPerson;
    }
    private void Health_OnDeath() {
        CustomLogger.Log(LogCategories.Camera, "Switched to Team spectator");
        currentCamera = CameraTypes.teamSpectator;
    }
    public void SetSpectator() {
        CustomLogger.Log(LogCategories.Camera, "Switched to Spectator");
        currentCamera = CameraTypes.Spectator;
    }
    public void SetLocked() {
        CustomLogger.Log(LogCategories.Camera, "Switched to Locked");
        currentCamera = CameraTypes.locked;
    }
}
