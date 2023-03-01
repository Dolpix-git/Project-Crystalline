using UnityEngine;

public class StatisticsManager : MonoBehaviour{
    private static StatisticsManager _instance;
    public static StatisticsManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<StatisticsManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("StatisticsManager"));
                    _instance = obj.AddComponent<StatisticsManager>();
                }
            }
            return _instance;
        }
    }

    // Stats(kills, assists, deaths, objectives(types), MVP's(types))
    // RandomStats(walkDistance,runDistance,crouchDistance,slideDistance,jumps,grenadesThrown(types), probs more)

    // Store statistics dictionary of (conn, Stats)
    // Update statistics dictionary based on player join or leave (server side)
    // Sync dictionary to clients

    // Store temp round statistics dictionary of (conn, Stats)


    // On game end, Reset statistics

    // On round end, Give MVP to player with the most points for that round, Reset temp statistics

    // On player death, Give point to killer, Give point to assister, Give point to player who died
    // how to get player death?
    // how to get their killer and kill assister

    // OnObjective, give point to completer


    // Function Reset statistics
}
