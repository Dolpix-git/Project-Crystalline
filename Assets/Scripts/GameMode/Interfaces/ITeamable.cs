using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITeamable {
    public Team GetTeamID();
    public void SetTeamID(Team teamID);
}

public enum Team {
    None,
    Defenders,
    Attackers
}
