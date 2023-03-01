using FishNet.Connection;
using System;

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
}

