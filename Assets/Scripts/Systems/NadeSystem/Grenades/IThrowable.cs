using FishNet.Connection;
using FishNet.Managing.Timing;
using UnityEngine;

public interface IThrowable {
    /// <summary>
    /// Initializes throwable with force.
    /// </summary>
    /// <param name="force"></param>
    void Initialize(Vector3 force, NetworkConnection conn);
}