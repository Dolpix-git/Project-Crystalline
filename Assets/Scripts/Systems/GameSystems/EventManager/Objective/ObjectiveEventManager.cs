using FishNet.Connection;
using System;
using UnityEngine;

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

    public void InvokeObjectiveComplete(Teams team, NetworkConnection net) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Objective complete Team:{team} Conn:{net}");
        OnObjectiveComplete?.Invoke(team, net);
    }

    public void InvokeObjectiveStarted(Teams team, NetworkConnection net) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Objective started Team:{team} Conn:{net}");
        OnObjectiveStarted?.Invoke(team, net);
    }
}
