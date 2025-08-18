#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class RosterDataBuilder
{
    [MenuItem("GridironGM/Data/Rebuild rosters_by_team.json…")]
    public static void Rebuild()
    {
        string teamsPath   = Path.Combine(Application.streamingAssetsPath, "teams.json");
        string rostersPath = Path.Combine(Application.streamingAssetsPath, "rosters_by_team.json");

        if (!File.Exists(teamsPath))
        {
            Debug.LogError($"[Rosters] Missing teams.json at {teamsPath}");
            return;
        }

        // Load teams.json (accept array or { "teams": [...] })
        var teamsJson = File.ReadAllText(teamsPath).TrimStart();
        if (teamsJson.StartsWith("[")) teamsJson = "{\"teams\":" + teamsJson + "}";
        var teamList = JsonUtility.FromJson<TeamDataList>(teamsJson)?.teams ?? new List<TeamData>();
        if (teamList.Count == 0) { Debug.LogError("[Rosters] No teams parsed from teams.json"); return; }

        // Load existing rosters if present
        var existing = new Dictionary<string, TeamRosterDTO>(StringComparer.OrdinalIgnoreCase);
        if (File.Exists(rostersPath))
        {
            var rjson = File.ReadAllText(rostersPath).TrimStart();
            if (rjson.StartsWith("[")) rjson = "{\"teams\":" + rjson + "}";
            var rootExisting = JsonUtility.FromJson<RostersRoot>(rjson);
            if (rootExisting?.teams != null)
                foreach (var tr in rootExisting.teams)
                    if (!string.IsNullOrEmpty(tr.abbreviation)) existing[tr.abbreviation] = tr;
        }

        // Ask how to treat existing
        int choice = EditorUtility.DisplayDialogComplex(
            "Rebuild rosters_by_team.json",
            $"Generate placeholder rosters for {teamList.Count} teams.\n\nWhat should I do with teams that already exist in the current file?",
            "Append Missing",           // 0
            "Overwrite All",            // 1
            "Cancel"                    // 2
        );
        if (choice == 2) return;
        bool overwrite = (choice == 1);

        // Build output
        var outList = new List<TeamRosterDTO>(teamList.Count);
        var rng = new System.Random(1337);
        foreach (var t in teamList)
        {
            if (!overwrite && existing.TryGetValue(t.abbreviation, out var keep))
            {
                outList.Add(keep);
            }
            else
            {
                outList.Add(GenerateRoster(t.abbreviation, rng));
            }
        }

        var outRoot = new RostersRoot { teams = outList };
        var outJson = JsonUtility.ToJson(outRoot, true);
        Directory.CreateDirectory(Application.streamingAssetsPath);
        File.WriteAllText(rostersPath, outJson);
        AssetDatabase.Refresh();

        Debug.Log($"[Rosters] Wrote {outList.Count} team rosters → {rostersPath}");
    }

    // --- Simple placeholder generator (28 players) ---
    static TeamRosterDTO GenerateRoster(string abbr, System.Random rng)
    {
        var roster = new TeamRosterDTO { abbreviation = abbr, players = new List<PlayerDTO>() };

        void Add(string pos, int count, int min = 67, int max = 83)
        {
            for (int i = 0; i < count; i++)
            {
                roster.players.Add(new PlayerDTO
                {
                    first_name = Pick(_first, rng),
                    last_name  = Pick(_last,  rng),
                    pos        = pos,
                    overall    = rng.Next(min, max + 1),
                    age        = rng.Next(21, 34)
                });
            }
        }

        Add("QB",   1, 72, 86);
        Add("RB",   2, 68, 82);
        Add("WR",   4, 68, 84);
        Add("TE",   2, 66, 81);
        Add("LT",   1, 70, 84);
        Add("LG",   1, 68, 82);
        Add("C",    1, 68, 82);
        Add("RG",   1, 68, 82);
        Add("RT",   1, 70, 84);
        Add("EDGE", 2, 70, 86);
        Add("IDL",  2, 68, 83);
        Add("LB",   4, 67, 83);
        Add("CB",   4, 67, 84);
        Add("S",    2, 68, 84);
        Add("K",    1, 65, 80);
        Add("P",    1, 65, 80);

        return roster;
    }

    static string Pick(string[] arr, System.Random r) => arr[r.Next(arr.Length)];

    static readonly string[] _first = {
        "Alex","Jordan","Chris","Taylor","Devin","Jalen","Marcus","Evan","Owen","Nate","Casey","Drew","Logan","Sam","Theo"
    };
    static readonly string[] _last = {
        "Johnson","Smith","Brown","Williams","Jones","Davis","Miller","Wilson","Moore","Taylor","Anderson","Thomas","Jackson","White","Harris"
    };
}
#endif
