using FishNet.Object.Prediction;
using UnityEngine;

public struct PlayerMoveData : IReplicateData {
    public bool Jump;
    public bool Crouch;
    public bool Sneak;
    public Vector2 Movement;
    public Vector3 CameraRight;
    public Vector3 CameraForward;
    public PlayerMoveData(bool jump, bool crouch, bool sprint, Vector2 movement, Vector3 cameraRight, Vector3 cameraForward) {
        Jump = jump;
        Crouch = crouch;
        Sneak = sprint;
        Movement = movement;

        CameraRight = cameraRight;
        CameraForward = cameraForward;

        _tick = 0;
    }

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

