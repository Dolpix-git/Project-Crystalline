using FishNet.Object.Prediction;
using UnityEngine;

public struct PlayerMoveData : IReplicateData {
    public bool Jump;
    public bool Crouch;
    public bool Sprint;
    public Vector2 Movement;
    public PlayerMoveData(bool jump, bool crouch, bool sprint, Vector2 movement) {
        Jump = jump;
        Crouch = crouch;
        Sprint = sprint;
        Movement = movement;

        _tick = 0;
    }

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

