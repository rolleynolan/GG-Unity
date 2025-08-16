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

    // Bridge wrapper that exposes team data via the core TeamProvider.
    public static class LeagueRepository
    {
        public static readonly List<TeamData> Teams = new List<TeamData>();
        public static bool Loaded { get { return Teams.Count > 0; } }

        public static void LoadTeams()
        {
            Teams.Clear();
            try
            {
                var abbrs = new TeamProvider().GetAllTeamAbbrs();
                if (abbrs != null && abbrs.Count > 0)
                {
                    foreach (var abbr in abbrs)
                    {
                        Teams.Add(new TeamData
                        {
                            abbreviation = abbr,
                            name = abbr,
                            city = string.Empty
                        });
                    }
                    GGLog.Info("[LeagueRepository] loaded " + Teams.Count + " teams via TeamProvider.");
                }
                else
                {
                    GGLog.Warn("[LeagueRepository] TeamProvider returned no abbreviations.");
                }
            }
            catch (Exception ex)
            {
                GGLog.Warn("[LeagueRepository] load failed: " + ex.Message);
            }
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
