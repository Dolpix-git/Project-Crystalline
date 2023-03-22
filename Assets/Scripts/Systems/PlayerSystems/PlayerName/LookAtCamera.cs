using UnityEngine;

public class LookAtCamera : MonoBehaviour{
    private Camera cam;

    private void Awake(){
        cam = Camera.main;
    }
    private void Update(){
        //transform.LookAt(camera.transform);
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }
}
