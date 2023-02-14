using UnityEngine;

public static class PlayerFunctionHelpers {
    public static Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
}
