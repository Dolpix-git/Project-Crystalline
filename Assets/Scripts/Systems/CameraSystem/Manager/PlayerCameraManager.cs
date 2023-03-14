using FishNet;
using FishNet.Connection;
using FishNet.Object;
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
    private CameraTypes currentCamera = CameraTypes.firstPerson;

    
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


        PlayerEventManager.Instance.OnPlayerClentConnected += Instance_OnPlayerClentConnected;
    }
    private void OnDestroy() {
        foreach (CameraBaseClass camera in cameraClasses.Values) {
            camera.DestroyCamera();
        }

        PlayerEventManager.Instance.OnPlayerClentConnected -= Instance_OnPlayerClentConnected;
    }


    private void LateUpdate() {
        cameraClasses[currentCamera].UpdateCamera();
    }


    private void Instance_OnPlayerClentConnected(NetworkObject obj) {
        NetworkConnection conn = obj.Owner;
        if (conn == InstanceFinder.ClientManager.Connection) {
            PlayerManager.Instance.players[conn].GetComponent<Health>().OnDeath += Health_OnDeath;
            PlayerManager.Instance.players[conn].GetComponent<Health>().OnRespawned += Health_OnRespawned;
            SetSpectator();
        }
    }

    private void Health_OnRespawned() {
        Log.LogMsg(LogCategories.Camera, "Switched to First person");
        currentCamera = CameraTypes.firstPerson;
        cameraClasses[currentCamera].SetCamera();
    }
    private void Health_OnDeath() {
        Log.LogMsg(LogCategories.Camera, "Switched to Team spectator");
        currentCamera = CameraTypes.teamSpectator;
        cameraClasses[currentCamera].SetCamera();
    }
    public void SetSpectator() {
        Log.LogMsg(LogCategories.Camera, "Switched to Spectator");
        currentCamera = CameraTypes.Spectator;
        cameraClasses[currentCamera].SetCamera();
    }
    public void SetLocked() {
        Log.LogMsg(LogCategories.Camera, "Switched to Locked");
        currentCamera = CameraTypes.locked;
        cameraClasses[currentCamera].SetCamera();
    }
}
