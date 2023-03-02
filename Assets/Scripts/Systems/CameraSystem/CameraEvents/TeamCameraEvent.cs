//using FishNet;
//using FishNet.Object;
//using FishNet.Transporting;
//using System;
//using UnityEngine;

//public class TeamCameraEvent : NetworkBehaviour {
//    [SerializeField] private NetworkObject nob;
//    public static event Action<Transform, PlayerNetworker> OnTeamCameraAdd;
//    public static event Action<Transform> OnTeamCameraRemove;
//    public static event Action<ITeamable> OnClientConnect;

//    public override void OnStartClient() {
//        base.OnStartClient();
//        if (base.IsOwner) {
//            ITeamable team = nob.GetComponent<ITeamable>();
//            OnClientConnect?.Invoke(team);
//        }
//    }
//    public override void OnStartServer() {
//        base.OnStartServer();
//        PlayerNetworker net = nob.GetComponent<PlayerNetworker>();
//        Debug.Log(net, transform);
//        OnTeamCameraAdd?.Invoke(transform, net);
//        InstanceFinder.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
//    }
//    private void OnDestroy() {
//        InstanceFinder.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
//    }
//    private void ServerManager_OnRemoteConnectionState(FishNet.Connection.NetworkConnection conn, RemoteConnectionStateArgs args) {
//        if (args.ConnectionState == RemoteConnectionState.Stopped) {
//            OnTeamCameraRemove?.Invoke(transform);
//        }
//    }
//}
