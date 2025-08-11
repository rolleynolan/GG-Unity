using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;
using GridironGM.Data;
using Newtonsoft.Json;

namespace GridironGM.Boot
{
    /// Ensures rosters_by_team.json includes ALL teams from teams.json.
    /// If the file is missing or a team key is missing/empty, we create placeholders.
    public static class RosterBootstrapper
    {
        private const string TeamsFile   = "teams.json";
        private const string RostersFile = "rosters_by_team.json";

        public static void EnsureRostersExist(int playersPerTeam = 12)
        {
            string streaming   = Application.streamingAssetsPath;
            string teamsPath   = Path.Combine(streaming, TeamsFile);
            string rostersPath = Path.Combine(streaming, RostersFile);

            if (!File.Exists(teamsPath))
            {
                Debug.LogError($"[RosterBootstrapper] Missing {TeamsFile} in StreamingAssets.");
                return;
            }

            List<TeamData> teams;
            try { teams = JsonLoader.LoadFromStreamingAssets<List<TeamData>>(TeamsFile); }
            catch (Exception ex) { Debug.LogError($"[RosterBootstrapper] Load teams.json failed: {ex.Message}"); return; }
            if (teams == null || teams.Count == 0) { Debug.LogError("[RosterBootstrapper] teams.json empty."); return; }

            // Load existing rosters if present; otherwise start fresh.
            RosterByTeam rosters = null;
            if (File.Exists(rostersPath))
            {
                try
                {
                    var text = File.ReadAllText(rostersPath);
                    rosters = string.IsNullOrWhiteSpace(text) ? null : JsonConvert.DeserializeObject<RosterByTeam>(text);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[RosterBootstrapper] Existing rosters unreadable. Will regenerate. {ex.Message}");
                }
            }
            rosters ??= new RosterByTeam();

            var rng = new System.Random(1337);
            int filled = 0;

            foreach (var t in teams)
            {
                if (!rosters.TryGetValue(t.abbreviation, out var list) || list == null || list.Count == 0)
                {
                    rosters[t.abbreviation] = MakePlaceholder(playersPerTeam, rng);
                    filled++;
                }
            }

            if (filled > 0 || !File.Exists(rostersPath))
            {
                try
                {
                    var json = JsonConvert.SerializeObject(rosters, Formatting.Indented);
                    File.WriteAllText(rostersPath, json);
                    Debug.Log($"[RosterBootstrapper] Filled {filled} missing teams. Total: {rosters.Count}.");
                }
                catch (Exception ex) { Debug.LogError($"[RosterBootstrapper] Write rosters failed: {ex.Message}"); }
            }
            else
            {
                Debug.Log($"[RosterBootstrapper] Found existing rosters for {rosters.Count} teams.");
            }
        }

        private static List<PlayerData> MakePlaceholder(int n, System.Random rng)
        {
            var list = new List<PlayerData>(Mathf.Max(n, 1));
            for (int i = 0; i < n; i++)
            {
                var pos = PickPos(i);
                int baseOvr = BaseOvrFor(pos);
                int ovr = Mathf.Clamp(baseOvr + rng.Next(-5, 6), 60, 92);
                int age = Mathf.Clamp(22 + rng.Next(0, 13), 20, 35);

                list.Add(new PlayerData
                {
                    id    = i + 1,
                    first = Firsts[rng.Next(Firsts.Length)],
                    last  = Lasts[rng.Next(Lasts.Length)],
                    pos   = pos,
                    ovr   = ovr,
                    age   = age
                });
            }
            return list;
        }

        private static string PickPos(int i)
        {
            string[] order = { "QB","RB","WR","WR","TE","OL","DL","LB","CB","S","K","P" };
            return order[i % order.Length];
        }
        private static int BaseOvrFor(string pos) =>
            pos switch { "QB"=>78, "WR"=>76, "RB"=>74, "CB"=>74, "TE"=>72, _=>70 };

        private static readonly string[] Firsts = { "Alex","Chris","Jordan","Taylor","Casey","Jalen","Devin","Micah" };
        private static readonly string[] Lasts  = { "Smith","Johnson","Brown","Williams","Jones","Davis","Miller","Moore" };
    }
}
