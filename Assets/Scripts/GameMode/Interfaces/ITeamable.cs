using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITeamable {
    public Teams GetTeamID();
    public void SetTeamID(Teams teamID);
}

public enum Teams {
    Solo,
    Defenders,
    Attackers
}
