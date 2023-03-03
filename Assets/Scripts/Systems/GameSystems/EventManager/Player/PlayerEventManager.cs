using FishNet.Connection;
using FishNet.Object;
using System;
using UnityEngine;

public class PlayerEventManager {
    private static PlayerEventManager _instance;
    public static PlayerEventManager Instance {
        get {
            if (_instance is null) {
                _instance = new PlayerEventManager();
            }
            return _instance;
        }
    }

    // The player who died, the player who killed them, and the player who assisted
    public event Action<NetworkConnection, NetworkConnection, NetworkConnection> OnPlayerDeath;

    public event Action<NetworkConnection> OnPlayerConnected;
    public event Action<NetworkObject> OnPlayerClentConnected;
    public event Action<NetworkConnection> OnPlayerDisconnected;

    public void InvokePlayerDeath(NetworkConnection dead, NetworkConnection killer, NetworkConnection assister) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Player has died  Dead:{dead?.ClientId} Killer:{killer?.ClientId} Assister:{assister?.ClientId}");
        OnPlayerDeath?.Invoke(dead,killer,assister);
    }
    public void InvokePlayerConnected(NetworkConnection conn) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Player connected {conn?.ClientId}");
        OnPlayerConnected?.Invoke(conn);
    }
    public void InvokePlayerClentConnected(NetworkObject obj) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Player connected {obj}");
        OnPlayerClentConnected?.Invoke(obj);
    }
    public void InvokePlayerdisconnected(NetworkConnection conn) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Player disconnected {conn?.ClientId}");
        OnPlayerDisconnected?.Invoke(conn);
    }
}

