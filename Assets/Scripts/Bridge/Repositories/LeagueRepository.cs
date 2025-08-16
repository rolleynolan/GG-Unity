using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using GG.Infra;

namespace GG.Bridge.Repositories
{
    [Serializable] public class TeamData { public string city; public string name; public string abbreviation; }
    [Serializable] public class TeamDataList { public TeamData[] teams; }

    public static class LeagueRepository
    {
        public static readonly List<TeamData> Teams = new();
        public static bool Loaded => Teams.Count > 0;

        /// <summary>Loads teams from StreamingAssets/teams.json (your existing file).</summary>
        public static void LoadTeams()
        {
            Teams.Clear();
            var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
            if (!File.Exists(path)) { GGLog.Warn("StreamingAssets/teams.json missing."); return; }
            var json = File.ReadAllText(path);
            var obj = JsonConvert.DeserializeObject<TeamDataList>(json);
            if (obj?.teams != null) Teams.AddRange(obj.teams);
            GGLog.Info($"[LeagueRepository] teams count: {Teams.Count}");
        }

        public static List<string> TeamAbbrs() => Teams.Select(t => t.abbreviation).ToList();
    }
}
