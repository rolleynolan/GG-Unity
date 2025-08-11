using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

// If your data models live in GridironGM.Data (TeamData, PlayerData, RosterByTeam, JsonLoader)
using GridironGM.Data;
using Newtonsoft.Json;

namespace GridironGM.Boot
{
    /// <summary>
    /// Ensures rosters_by_team.json exists (and isn't empty) when the Team Selection scene opens.
    /// If the file is missing/empty, it creates a simple placeholder roster for every team in teams.json.
    /// </summary>
    public static class RosterBootstrapper
    {
        private const string TeamsFile = "teams.json";
        private const string RostersFile = "rosters_by_team.json";

        /// <summary>
        /// Call this once before you try to read rosters. Safe to call multiple times.
        /// </summary>
        public static void EnsureRostersExist(int playersPerTeam = 8)
        {
            string streaming = Application.streamingAssetsPath;
            string teamsPath  = Path.Combine(streaming, TeamsFile);
            string rostersPath = Path.Combine(streaming, RostersFile);

            // 1) If a non-empty rosters file already exists, do nothing.
            if (File.Exists(rostersPath))
            {
                try
                {
                    var text = File.ReadAllText(rostersPath);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        // quick sanity: must deserialize to a dictionary-ish structure
                        var probe = JsonConvert.DeserializeObject<RosterByTeam>(text);
                        if (probe != null && probe.Count > 0)
                        {
                            Debug.Log($"[RosterBootstrapper] Found existing rosters for {probe.Count} teams.");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[RosterBootstrapper] Existing rosters file unreadable, will regenerate. {ex.Message}");
                }
            }

            // 2) Load team list (required)
            if (!File.Exists(teamsPath))
            {
                Debug.LogError($"[RosterBootstrapper] Missing {TeamsFile} in StreamingAssets. Cannot generate rosters.");
                return;
            }

            List<TeamData> teams;
            try
            {
                teams = JsonLoader.LoadFromStreamingAssets<List<TeamData>>(TeamsFile);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RosterBootstrapper] Failed to load teams.json: {ex.Message}");
                return;
            }

            if (teams == null || teams.Count == 0)
            {
                Debug.LogError("[RosterBootstrapper] teams.json is empty; cannot generate rosters.");
                return;
            }

            // 3) Generate a simple placeholder roster per team
            var rng = new System.Random(1337);
            var output = new RosterByTeam();

            foreach (var t in teams)
            {
                var list = new List<PlayerData>(PlayerDataCapacity(playersPerTeam)); // small optimization
                for (int i = 0; i < playersPerTeam; i++)
                {
                    var pos = PickPos(i);                   // rotate positions for variety
                    int baseOvr = BaseOvrFor(pos);         // give QBs/WRs slightly higher base
                    int ovr = Mathf.Clamp(baseOvr + rng.Next(-5, 6), 60, 92);
                    int age = Mathf.Clamp(22 + rng.Next(0, 13), 20, 35);

                    list.Add(new PlayerData
                    {
                        id = i + 1,
                        first = SampleFirst(rng),
                        last  = SampleLast(rng),
                        pos = pos,
                        ovr = ovr,
                        age = age
                    });
                }
                output[t.abbreviation] = list;
            }

            // 4) Write rosters_by_team.json
            try
            {
                var json = JsonConvert.SerializeObject(output, Formatting.Indented);
                File.WriteAllText(rostersPath, json);
                Debug.Log($"[RosterBootstrapper] Generated {playersPerTeam} placeholder players for {output.Count} teams.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RosterBootstrapper] Failed to write {RostersFile}: {ex.Message}");
            }
        }

        // --- Helpers below (simple + readable) ---

        private static int PlayerDataCapacity(int n) => Mathf.Max(n, 1);

        private static string PickPos(int i)
        {
            // cycle through common positions so the list looks sane
            string[] order = { "QB", "RB", "WR", "WR", "TE", "OL", "DL", "LB", "CB", "S", "K", "P" };
            return order[i % order.Length];
        }

        private static int BaseOvrFor(string pos)
        {
            switch (pos)
            {
                case "QB": return 78;
                case "WR": return 76;
                case "RB": return 74;
                case "CB": return 74;
                case "TE": return 72;
                default:   return 70;
            }
        }

        // Tiny name pools just for placeholders
        private static readonly string[] Firsts = { "Alex", "Chris", "Jordan", "Taylor", "Casey", "Jalen", "Devin", "Micah" };
        private static readonly string[] Lasts  = { "Smith", "Johnson", "Brown", "Williams", "Jones", "Davis", "Miller", "Moore" };

        private static string SampleFirst(System.Random rng) => Firsts[rng.Next(Firsts.Length)];
        private static string SampleLast(System.Random rng)  => Lasts[rng.Next(Lasts.Length)];
    }
}
