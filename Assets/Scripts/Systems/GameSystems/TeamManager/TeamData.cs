public struct TeamData {
    public string name;
    public Objectives objective;
    public int points;

    public TeamData(string name, Objectives objective, int points) {
        this.name = name;
        this.objective = objective;
        this.points = points;
    }
}