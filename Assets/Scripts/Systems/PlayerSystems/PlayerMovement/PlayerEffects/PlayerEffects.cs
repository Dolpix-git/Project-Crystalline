using FishNet.Object;
using Unity.Mathematics;
using UnityEngine;

public class PlayerEffects : NetworkBehaviour{
    [Header("GDSS")]
    [SerializeField] private LayerMask probeMask = -1;
    [SerializeField] private float probeDistance = 0.1f; // the distance check for how far the controller will snap to a surface
    [SerializeField] private float maxSnapSpeed = 100f; // the max speed of when the controller will not snap
    [SerializeField] private float maxSlope = 40f;

    [Header("Locomotion")]
    [SerializeField] private AnimationCurve groundAcc;
    [SerializeField] private AnimationCurve airAcc;

    [Header("Walking")]
    [SerializeField] private float walkingMaxSpeed = 6f;
    [SerializeField] private float4 walkigSpeedClamp = new float4(-10, 10, -10, 10);

    [Header("Sneeking")]
    [SerializeField] private float sneakingMaxSpeed = 3f;
    [SerializeField] private float4 sneakingSpeedClamp = new float4(-10, 10, -10, 10);

    [Header("Crouch")]
    [SerializeField] private float crouchDelta = 0.8f;
    [SerializeField] private float crouchMaxSpeed = 7f;
    [SerializeField] private float4 crouchSpeedClamp = new float4(-10, 10, -10, 10);

    public LayerMask ProbeMask { get => probeMask; }
    public float ProbeDistance { get => probeDistance; }
    public float MaxSnapSpeed { get => maxSnapSpeed; }
    public float MaxSlope { get => maxSlope; }


    public float GroundAcc(float eval) { return groundAcc.Evaluate(eval); }
    public float AirAcc(float eval) { return airAcc.Evaluate(eval); }

    public float WalkingMaxSpeed { get => walkingMaxSpeed; }
    public float4 WalkigSpeedClamp { get => walkigSpeedClamp; }


    public float SneakMaxSpeed { get => sneakingMaxSpeed; set => sneakingMaxSpeed = value; }
    public float4 SneakSpeedClamp { get => sneakingSpeedClamp; set => sneakingSpeedClamp = value; }


    public float CrouchDelta { get => crouchDelta; }
    public float CrouchMaxSpeed { get => crouchMaxSpeed; } 
    public float4 CrouchSpeedClamp { get => crouchSpeedClamp; }

}
