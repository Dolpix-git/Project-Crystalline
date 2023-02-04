using FishNet.Object.Prediction;
using UnityEngine;

public struct PlayerReconcileData : IReconcileData {
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;
    public Vector3 AngularVelocity;
    public PlayerReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) {
        Position = position;
        Rotation = rotation;
        Velocity = velocity;
        AngularVelocity = angularVelocity;

        _tick = 0;
    }

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}