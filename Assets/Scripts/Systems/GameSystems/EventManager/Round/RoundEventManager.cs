using System;
using UnityEngine;

public class RoundEventManager {
    private static RoundEventManager _instance;
    public static RoundEventManager Instance {
        get {
            if (_instance is null) {
                _instance = new RoundEventManager();
            }
            return _instance;
        }
    }

    public event Action OnRoundStart;

    // The team that won the round, if null then no team has won that round
    public event Action<Teams> OnRoundEnd;

    public event Action OnTeamFlipFlop;

    public void InvokeRoundStart() {
        CustomLogger.Log(LogCategories.SystEventManager, "Round Start");
        OnRoundStart?.Invoke();
    }
    public void InvokeRoundEnd(Teams team) {
        CustomLogger.Log(LogCategories.SystEventManager, $"Round End with {team} winning");
        OnRoundEnd?.Invoke(team);
    }
    public void InvokeTeamFlipFlop() {
        CustomLogger.Log(LogCategories.SystEventManager, "Team flip flop");
        OnTeamFlipFlop?.Invoke();
    }
}

