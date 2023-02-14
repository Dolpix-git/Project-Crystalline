using FishNet.Managing.Timing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IThrowable {
    /// <summary>
    /// Initializes throwable with force.
    /// </summary>
    /// <param name="force"></param>
    void Initialize(PreciseTick pt, Vector3 force);
}