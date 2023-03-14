using FishNet;
using FishNet.Connection;
using System.Linq;
using UnityEngine;

public class TeamSpectatorCamera : CameraBaseClass {
    private PlayerHead currentHead;
    private int pointerIndex;
    float counter = 0;
    [SerializeField] float rotationLerp = 0.5f;
    public override void SetCamera() {
        NextTeamCamera();
    }
    public override void DestroyCamera() { }
    
    public override void UpdateCamera() {
        counter += 1;
        if (counter > 100) {
            counter = 0;
            NextTeamCamera();
        }
        if (currentHead != null) {
            CameraPosition();
            CameraRotation();
        }
    }

    /// <summary>
    /// Updates camera position
    /// </summary>
    private void CameraPosition() {
        PlayerCameraManager.transform.position = currentHead.head.position;
    }

    /// <summary>
    /// Updates camera rotation
    /// </summary>
    private void CameraRotation() {
        Quaternion newLookQ = Quaternion.LookRotation(currentHead.GetPlayerForwardVector3(), Vector3.up);
        Quaternion slerpQ = Quaternion.Lerp(PlayerCameraManager.transform.rotation, newLookQ, rotationLerp);
        PlayerCameraManager.transform.rotation = slerpQ;
    }

    public void NextTeamCamera() {
        Log.LogMsg($"Looking for the next team camera");
        int tempIndex = pointerIndex + 1;

        NetworkConnection ownerConn = InstanceFinder.ClientManager.Connection;
        Teams ownerTeam = TeamManager.Instance.playerTeams[ownerConn];

        NetworkConnection[] conn = PlayerManager.Instance.players.Keys.ToArray();
        while (pointerIndex != tempIndex) {
            if (tempIndex >= conn.Length) {
                tempIndex = 0;
            }
            NetworkConnection currentConn = conn[tempIndex];
            if (TeamManager.Instance.playerTeams[currentConn] == ownerTeam && currentConn != ownerConn) {
                currentHead = PlayerManager.Instance.players[currentConn].GetComponent<PlayerHead>();
                pointerIndex = tempIndex;
                break;
            }
            tempIndex++;
        }
    }
    public void PreviousTeamCamera() {

    }
}