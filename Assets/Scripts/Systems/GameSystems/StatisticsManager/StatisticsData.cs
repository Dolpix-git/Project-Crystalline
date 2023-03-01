public struct StatisticsData {
    public int kills;
    public int assists;
    public int deaths;
    public int objectives;
    public int MVPs;

    public void Reset() {
        kills = 0;
        assists = 0;
        deaths = 0;
        objectives = 0;
        MVPs = 0;
    }
}
public struct RoundStatisticsData {
    public int kills;
    public int assists;
    public int deaths;
    public int objectives;

    public int AssessValue() {
        int i = 0;
        i += kills * 5;
        i += assists * 3;
        i += deaths * -1;
        i += objectives * 4;
        return i;
    }
}