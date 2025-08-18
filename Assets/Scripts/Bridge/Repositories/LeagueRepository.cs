using System;
using System.Collections.Generic;

namespace GG.Bridge.Repositories
{
    [Serializable]
    public class TeamData
    {
        public string city;
        public string name;
        public string abbreviation;
    }

    public static class LeagueRepository
    {
        public static readonly List<TeamData> Teams = new List<TeamData>();
        public static bool Loaded { get { return Teams.Count > 0; } }

        public static void LoadTeams()
        {
            Teams.Clear();
            Teams.Add(new TeamData { abbreviation = "ATL", name = "ATL", city = string.Empty });
            Teams.Add(new TeamData { abbreviation = "PHI", name = "PHI", city = string.Empty });
            Teams.Add(new TeamData { abbreviation = "DAL", name = "DAL", city = string.Empty });
            Teams.Add(new TeamData { abbreviation = "NYG", name = "NYG", city = string.Empty });
        }

        public static List<string> TeamAbbrs()
        {
            if (!Loaded) LoadTeams();
            var list = new List<string>(Teams.Count);
            foreach (var t in Teams)
            {
                if (!string.IsNullOrEmpty(t.abbreviation))
                    list.Add(t.abbreviation);
            }
            return list;
        }
    }
}
