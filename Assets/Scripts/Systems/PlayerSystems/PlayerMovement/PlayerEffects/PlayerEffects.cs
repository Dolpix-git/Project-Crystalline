using FishNet.Object;
using Unity.Mathematics;
using UnityEngine;

public class PlayerEffects : NetworkBehaviour{
    [Header("GDSS")]
    [SerializeField] private LayerMask probeMask = -1;
    [SerializeField] private float probeDistance = 0.1f; // the distance check for how far the controller will snap to a surface
    [SerializeField] private float maxSnapSpeed = 100f; // the max speed of when the controller will not snap
    [SerializeField] private float maxSlope = 40f;

    [Header("Idle")]
    [SerializeField] private float idleGroundAcc = 90;
    [SerializeField] private float idleAirAcc = 10;

    [Header("Walking")]
    [SerializeField] private float walkingGroundAcc = 90;
    [SerializeField] private float walkingAirAcc = 10;
    [SerializeField] private float walkingMaxSpeed = 3.5f;
    [SerializeField] private float4 walkigSpeedClamp = new float4(-10, 10, -10, 10);

    [Header("Running")]
    [SerializeField] private float runningGroundAcc = 90;
    [SerializeField] private float runningAirAcc = 10;
    [SerializeField] private float runningMaxSpeed = 7f;
    [SerializeField] private float4 runningSpeedClamp = new float4(-10, 10, -10, 10);

    [Header("Crouch")]
    [SerializeField] private float crouchDelta = 0.8f;
    [SerializeField] private float crouchGroundAcc = 90;
    [SerializeField] private float crouchAirAcc = 10;
    [SerializeField] private float crouchMaxSpeed = 7f;
    [SerializeField] private float4 crouchSpeedClamp = new float4(-10, 10, -10, 10);

    [Header("sliding")]
    [SerializeField] private float slidingActivationSpeed = 6;
    [SerializeField] private float slidingDeactivationSpeed = 3;
    [SerializeField, Range(0, 1)] private float slidingFriction = 0.9f;
    [SerializeField] private float slidingGroundAcc = 90;
    [SerializeField] private float slidingAirAcc = 10;
    [SerializeField] private float slidingMaxSpeed = 7f;
    [SerializeField] private float4 slidingSpeedClamp = new float4(-10, 10, -10, 10);

    public LayerMask ProbeMask { get => probeMask; }
    public float ProbeDistance { get => probeDistance; }
    public float MaxSnapSpeed { get => maxSnapSpeed; }
    public float MaxSlope { get => maxSlope; }

    public float IdleGroundAcc { get => idleGroundAcc; }
    public float IdleAirAcc { get => idleAirAcc; }

    public float WalkingGroundAcc { get => walkingGroundAcc; }
    public float WalkingAirAcc { get => walkingAirAcc; }
    public float WalkingMaxSpeed { get => walkingMaxSpeed; }
    public float4 WalkigSpeedClamp { get => walkigSpeedClamp; }

    public float RunningGroundAcc { get => runningGroundAcc; }
    public float RunningAirAcc { get => runningAirAcc; }
    public float RunningMaxSpeed { get => runningMaxSpeed; }
    public float4 RunningSpeedClamp { get => runningSpeedClamp; }

    public float CrouchDelta { get => crouchDelta; }
    public float CrouchGroundAcc { get => crouchGroundAcc; }
    public float CrouchAirAcc { get => crouchAirAcc; }
    public float CrouchMaxSpeed { get => crouchMaxSpeed; }
    public float4 CrouchSpeedClamp { get => crouchSpeedClamp; }

    public float SlidingActivationSpeed { get => slidingActivationSpeed; }
    public float SlidingDeactivationSpeed { get => slidingDeactivationSpeed; }
    public float SlidingFriction { get => slidingFriction; }
    public float SlidingGroundAcc { get => slidingGroundAcc; }
    public float SlidingAirAcc { get => slidingAirAcc; }
    public float SlidingMaxSpeed { get => slidingMaxSpeed; }
    public float4 SlidingSpeedClamp { get => slidingSpeedClamp; }
    
}
