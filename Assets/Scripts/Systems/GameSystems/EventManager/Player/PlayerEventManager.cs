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
        Log.LogMsg(LogCategories.SystEventManager, $"Player has died  Dead:{dead?.ClientId} Killer:{killer?.ClientId} Assister:{assister?.ClientId}");
        OnPlayerDeath?.Invoke(dead,killer,assister);
    }
    public void InvokePlayerConnected(NetworkConnection conn) {
        Log.LogMsg(LogCategories.SystEventManager, $"Player connected {conn?.ClientId}");
        OnPlayerConnected?.Invoke(conn);
    }
    /// <summary>
    /// Exicutes on clients when a player connects.
    /// </summary>
    /// <param name="obj"></param>
    public void InvokePlayerClentConnected(NetworkObject obj) {
        Log.LogMsg(LogCategories.SystEventManager, $"Player connected {obj}");
        OnPlayerClentConnected?.Invoke(obj);
    }
    public void InvokePlayerDisconnected(NetworkConnection conn) {
        Log.LogMsg(LogCategories.SystEventManager, $"Player disconnected {conn?.ClientId}");
        OnPlayerDisconnected?.Invoke(conn);
    }
}

