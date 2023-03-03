using FishNet.Connection;
using FishNet.Object;
using System.Linq;
using UnityEngine;

public class TeamSpectatorCamera : CameraBaseClass {
    private PlayerHead currentHead;
    private int pointerIndex;
    float counter = 0;

    public override void SetCamera() {
        NextTeamCamera();
    }
    public override void DestroyCamera() {
        
    }
    
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
        PlayerCameraManager.transform.rotation = Quaternion.LookRotation(currentHead.GetPlayerForwardVector3(), Vector3.up);
    }

    public void NextTeamCamera() {
        CustomLogger.Log($"Looking for the next team camera");
        int tempIndex = pointerIndex + 1;
        Teams ownerTeam = TeamManager.Instance.playerTeams[LocalConnection];
        NetworkConnection[] conn = PlayerManager.Instance.players.Keys.ToArray();
        NetworkObject[] netObj = PlayerManager.Instance.players.Values.ToArray();
        Teams[] team = TeamManager.Instance.playerTeams.Values.ToArray();
        while (pointerIndex != tempIndex) {
            if (tempIndex >= conn.Length) {
                tempIndex = 0;
            }
            if (team[tempIndex] == ownerTeam) {
                currentHead = netObj[tempIndex].GetComponent<PlayerHead>();
                pointerIndex = tempIndex;
                break;
            }
            tempIndex++;
        }
    }
    public void PreviousTeamCamera() {

    }
}