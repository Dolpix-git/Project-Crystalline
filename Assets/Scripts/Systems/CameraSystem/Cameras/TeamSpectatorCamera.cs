//using FishNet.Object.Synchronizing;
//using System.Collections.Generic;
//using UnityEngine;

//public class TeamSpectatorCamera : CameraBaseClass {
//    [SyncObject]
//    private readonly SyncList<TeamHeadData> heads = new SyncList<TeamHeadData>();
//    private ITeamable team = null;
//    private int indexOfHead = 0;
//    private void Start() {
//        //TeamCameraEvent.OnTeamCameraAdd += TeamCamera_OnTeamCamera;
//        //TeamCameraEvent.OnTeamCameraRemove += TeamCamera_OnTeamCameraRemove;
//        //TeamCameraEvent.OnClientConnect += TeamCameraEvent_OnClientConnect;
//    }
//    public override void DestroyCamera() { 
//        //TeamCameraEvent.OnTeamCameraAdd -= TeamCamera_OnTeamCamera;
//        //TeamCameraEvent.OnTeamCameraRemove -= TeamCamera_OnTeamCameraRemove;
//        //TeamCameraEvent.OnClientConnect -= TeamCameraEvent_OnClientConnect;
//        heads.Clear();
//    }

//    private void TeamCamera_OnTeamCamera(Transform obj, PlayerNetworker net) {
//        heads.Add(new TeamHeadData(obj, net));
//        indexOfHead = heads.Count-1;
//    }

//    private void TeamCamera_OnTeamCameraRemove(Transform obj) {
//        for (int i = 0; i < heads.Count; i++) {
//            if (heads[i].headTransform == obj) {
//                heads.RemoveAt(i);
//            }
//        }
//    }
//    private void TeamCameraEvent_OnClientConnect(ITeamable obj) {
//        team = obj;
//    }





//    public override void UpdateCamera() {
//        CameraPosition();
//        CameraRotation();
//    }


//    /// <summary>
//    /// Updates camera position
//    /// </summary>
//    private void CameraPosition() {
//        if (heads.Count == 0 || indexOfHead >= heads.Count) { return; }

//        PlayerCameraManager.transform.position = heads[indexOfHead].headTransform.position;
//    }

//    /// <summary>
//    /// Updates camera rotation
//    /// </summary>
//    private void CameraRotation() {
//        if (heads.Count == 0 || indexOfHead >= heads.Count) { return; }

//        PlayerCameraManager.transform.rotation = Quaternion.LookRotation(heads[indexOfHead].headRotation.LastMove.CameraForward,Vector3.up);
//    }



//    public void NextCam() {

//    }
//    public void PrevCam() {

//    }
//}

//public struct TeamHeadData {
//    public Transform headTransform;
//    public PlayerNetworker headRotation;

//    public TeamHeadData(Transform headTransform, PlayerNetworker headRotation) {
//        this.headTransform = headTransform;
//        this.headRotation = headRotation;
//    }
//}