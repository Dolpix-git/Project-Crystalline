using UnityEngine;

public class Zone : MonoBehaviour, IZonable{
    [SerializeField] private Zones zone;

    public Zones GetZone() {
        return zone;
    }
}
