using FishNet;
using FishNet.Connection;
using System.Linq;
using UnityEngine;

public class TeamSpectatorCamera : CameraBaseClass {
    [SerializeField] float rotationLerp = 0.5f;

    private PlayerHead currentHead;
    private int pointerIndex;


    private void Awake() {
        CameraInputManager.Instance.OnNextCam += NextTeamCamera;
        CameraInputManager.Instance.OnPrevCam += PreviousTeamCamera;
    }
    public override void SetCamera() {
        NextTeamCamera();
    }
    public override void DestroyCamera() { }
    
    public override void UpdateCamera() {
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
        if (playerCameraManager.CurrentCamera != CameraTypes.teamSpectator) return;
        Log.LogMsg($"Looking for the next team camera");
        NetworkConnection ownerConn = InstanceFinder.ClientManager.Connection;
        Teams ownerTeam = TeamManager.Instance.playerTeams[ownerConn];

        NetworkConnection[] conn = PlayerManager.Instance.players.Keys.ToArray();

        pointerIndex++;
        for (int i = 0; i < conn.Length; i++) {
            int indexOffset = (pointerIndex + i) % conn.Length;

            NetworkConnection currentConn = conn[indexOffset];
            if (TeamManager.Instance.playerTeams[currentConn] == ownerTeam && currentConn != ownerConn) {
                currentHead = PlayerManager.Instance.players[currentConn].GetComponent<PlayerHead>();
                pointerIndex = indexOffset;
                break;
            }
        }
    }
    public void PreviousTeamCamera() {
        if (playerCameraManager.CurrentCamera != CameraTypes.teamSpectator) return;
        Log.LogMsg($"Looking for the previous team camera");
        NetworkConnection ownerConn = InstanceFinder.ClientManager.Connection;
        Teams ownerTeam = TeamManager.Instance.playerTeams[ownerConn];

        NetworkConnection[] conn = PlayerManager.Instance.players.Keys.ToArray();

        pointerIndex--;
        for (int i = 0; i < conn.Length; i++) {
            int indexOffset = (pointerIndex - i);
            if (indexOffset < 0) indexOffset += conn.Length;

            NetworkConnection currentConn = conn[indexOffset];
            if (TeamManager.Instance.playerTeams[currentConn] == ownerTeam && currentConn != ownerConn) {
                currentHead = PlayerManager.Instance.players[currentConn].GetComponent<PlayerHead>();
                pointerIndex = indexOffset;
                break;
            }
        }
    }
}