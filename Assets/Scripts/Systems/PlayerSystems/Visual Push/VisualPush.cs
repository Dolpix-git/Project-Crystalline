using FishNet.Object;
using UnityEngine;

public class VisualPush : NetworkBehaviour{
    void OnTriggerStay(Collider other) {
        if (!base.IsServer) return;
        if (other.attachedRigidbody != null) {
            Vector3 dir = other.gameObject.transform.position - transform.position;
            dir.y = 0;
            other.attachedRigidbody.velocity = dir.normalized * 2;
            other.attachedRigidbody.position += dir.normalized * 0.04f;
        }
    }
}
