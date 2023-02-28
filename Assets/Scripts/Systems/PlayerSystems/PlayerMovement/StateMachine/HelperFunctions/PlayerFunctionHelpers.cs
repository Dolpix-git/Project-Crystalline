using UnityEngine;

public static class PlayerFunctionHelpers {
    /// <summary>
    /// Returns the direction along the normal plane inputed. Used for getting players inputs relative to the players look vectors.
    /// </summary>
    /// <param name="direction">Direction of vector</param>
    /// <param name="normal">Vector of the plane you want to project on</param>
    /// <returns>Normalized projected direction on the plane</returns>
    public static Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
}
