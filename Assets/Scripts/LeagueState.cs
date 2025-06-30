using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string name;
    public string position;
    public string college;
    public int overall;
}

[Serializable]
public class TeamRosterEntry
{
    public string team;
    public List<PlayerData> players;
}

[Serializable]
public class LeagueState
{
    public List<TeamRosterEntry> teams;
    public List<PlayerData> free_agents;
}

[Serializable]
public class LeagueStateWrapper
{
    public LeagueState leagueState;
}
