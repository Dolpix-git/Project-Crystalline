using FishNet.Connection;
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
    public event Action<NetworkConnection> OnPlayerDisconnected;

    public void InvokePlayerDeath(NetworkConnection dead, NetworkConnection killer, NetworkConnection assister) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Player has died  Dead:{dead} Killer:{killer} Assister:{assister}");
        OnPlayerDeath?.Invoke(dead,killer,assister);
    }
    public void InvokePlayerConnected(NetworkConnection conn) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Player connected {conn}");
        OnPlayerConnected?.Invoke(conn);
    }
    public void InvokePlayerdisconnected(NetworkConnection conn) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Player disconnected {conn}");
        OnPlayerDisconnected?.Invoke(conn);
    }
}

