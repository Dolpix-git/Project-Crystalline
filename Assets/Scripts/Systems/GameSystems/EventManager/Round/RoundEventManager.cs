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
        Log.LogMsg(LogCategories.SystEventManager, "Round Start");
        OnRoundStart?.Invoke();
    }
    public void InvokeRoundEnd(Teams team) {
        Log.LogMsg(LogCategories.SystEventManager, $"Round End with {team} winning");
        OnRoundEnd?.Invoke(team);
    }
    public void InvokeTeamFlipFlop() {
        Log.LogMsg(LogCategories.SystEventManager, "Team flip flop");
        OnTeamFlipFlop?.Invoke();
    }
}

