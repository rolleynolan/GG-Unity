using System;
using System.Collections.Generic;

namespace GridironGM.Data.Legacy
{
    [Serializable]
    public class LegacyPlayerData
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
        public List<LegacyPlayerData> players;
    }

    [Serializable]
    public class LeagueState
    {
        public List<TeamRosterEntry> teams;
        public List<LegacyPlayerData> free_agents;
    }

    [Serializable]
    public class LeagueStateWrapper
    {
        public LeagueState leagueState;
    }
}
