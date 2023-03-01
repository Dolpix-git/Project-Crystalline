using FishNet.Connection;
using System;

public class ObjectiveEventManager {
    private static ObjectiveEventManager _instance;
    public static ObjectiveEventManager Instance {
        get {
            if (_instance is null) {
                _instance = new ObjectiveEventManager();
            }
            return _instance;
        }
    }

    // Passes in the team that called the action, and the person who started it.
    // if networkConnection is null, then no player completed the objective but a team had.
    public event Action<Teams, NetworkConnection> OnObjectiveComplete;
    public event Action<Teams, NetworkConnection> OnObjectiveStarted;
}
