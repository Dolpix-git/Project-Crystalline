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
    private CameraTypes currentCamera = CameraTypes.firstPerson;

    
    public float CameraRotationSpeed { get => cameraRotationSpeed; set => cameraRotationSpeed = value; }
    public float MaxCameraDelta { get => maxCameraDelta; set => maxCameraDelta = value; }
    public float MaxCameraAngle { get => maxCameraAngle; set => maxCameraAngle = value; }

    private void Awake() {
        cameraClasses.Add(CameraTypes.firstPerson, new FirstPersonCamera(this));
    }
    private void OnDestroy() {
        foreach (CameraBaseClass camera in cameraClasses.Values) {
            camera.DestroyCamera();
        }   
    }

    private void LateUpdate() {
        cameraClasses[currentCamera].UpdateCamera();
    }

    private void Health_OnRespawned() {
        currentCamera = CameraTypes.firstPerson;
    }
    private void Health_OnDeath() {
        currentCamera = CameraTypes.teamSpectator;
    }
    public void SetSpectator() {
        // kill player

        // disable player

        currentCamera = CameraTypes.Spectator;
    }
    public void SetLocked() {
        // kill player

        // disable player

        currentCamera = CameraTypes.locked;
    }
}
