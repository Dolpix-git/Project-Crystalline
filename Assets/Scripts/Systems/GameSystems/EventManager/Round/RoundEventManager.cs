using System;

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

    public event Action<Teams,Teams> OnTeamFlipFlop;
}

