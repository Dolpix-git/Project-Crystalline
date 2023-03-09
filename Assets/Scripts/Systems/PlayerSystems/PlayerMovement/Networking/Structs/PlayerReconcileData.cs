using FishNet.Object.Prediction;
using UnityEngine;

public struct PlayerReconcileData : IReconcileData {
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;
    public Vector3 AngularVelocity;
    public Vector3 ColliderCenter;
    public float ColliderHeight;
    public PlayerStates SuperPlayerState;
    public PlayerStates SubPlayerState;
    public PlayerReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, Vector3 colliderCenter, float colliderHeight, PlayerStates superPlayerState, PlayerStates subPlayerState) {
        Position = position;
        Rotation = rotation;
        Velocity = velocity;
        AngularVelocity = angularVelocity;
        ColliderCenter = colliderCenter;
        ColliderHeight = colliderHeight;
        SuperPlayerState = superPlayerState;
        SubPlayerState = subPlayerState;

        _tick = 0;
    }

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}