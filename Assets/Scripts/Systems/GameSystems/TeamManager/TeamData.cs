public struct TeamData {
    public string name;
    public Objectives objective;
    public int points;

    public TeamData(string name, Objectives obj) {
        this.name = name;
        this.objective = obj;
        this.points = 0;
    }

    public void Reset() {
        name = "";
        objective = Objectives.None;
        points = 0;
    }
}