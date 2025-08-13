using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LeagueRepository
{
    public static Team[] GetTeams()
    {
        var json = LoadJson("teams.json", new[] { "Config/teams", "teams" }, out var src);
        if (string.IsNullOrEmpty(json)) return Array.Empty<Team>();

        json = json.Trim();
        Team[] teams = json.StartsWith("[")
            ? JsonHelper.FromJson<Team>(json)
            : JsonUtility.FromJson<TeamList>(json)?.teams ?? Array.Empty<Team>();

        Debug.Log($"[LeagueRepository] Teams count: {teams.Length}");
        return teams;
    }

    public static List<Player> GetRoster(string abbr)
    {
        if (string.IsNullOrEmpty(abbr)) return new List<Player>();
        var json = LoadJson("rosters_by_team.json", new[] { "Generated/rosters_by_team", "rosters_by_team" }, out var src);
        if (!string.IsNullOrEmpty(json)) return ParseRoster(json, abbr);
        Debug.LogWarning("[LeagueRepository] rosters_by_team.json not found. Returning placeholder roster.");
        return PlaceholderRoster();
    }

    public static List<TeamGame> GetTeamSchedule(string abbr)
    {
        if (string.IsNullOrEmpty(abbr)) return new List<TeamGame>();
        var json = LoadJson("schedule_by_team.json", new[] { "Generated/schedule_by_team", "schedule_by_team" }, out var src);
        if (!string.IsNullOrEmpty(json)) return ParseSchedule(json, abbr);
        Debug.LogWarning("[LeagueRepository] schedule_by_team.json not found.");
        return new List<TeamGame>();
    }

    // ===== helpers =====
    public static string LoadJson(string fileName, string[] resourceCandidates, out string source)
    {
        source = null;

        // persistent data path
        var path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            source = path;
            Debug.Log($"[LeagueRepository] Loaded {fileName} from {source} ({json.Length} bytes)");
            return json;
        }

        // streaming assets (synchronous in editor/standalone)
        path = Path.Combine(Application.streamingAssetsPath, fileName);
        if ((Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.LinuxPlayer) && File.Exists(path))
        {
            var json = File.ReadAllText(path);
            source = path;
            Debug.Log($"[LeagueRepository] Loaded {fileName} from {source} ({json.Length} bytes)");
            return json;
        }

        // resources
        foreach (var candidate in resourceCandidates)
        {
            var ta = Resources.Load<TextAsset>(candidate);
            if (ta != null && !string.IsNullOrEmpty(ta.text))
            {
                source = "Resources/" + candidate;
                var json = ta.text;
                Debug.Log($"[LeagueRepository] Loaded {fileName} from {source} ({json.Length} bytes)");
                return json;
            }
        }

        Debug.LogWarning($"[LeagueRepository] {fileName} not found.");
        return null;
    }

    private static List<Player> ParseRoster(string json, string abbr)
    {
        try
        {
            // manual parse for { "ATL":[...], "BUF":[...] }
            var root = JsonUtility.FromJson<Wrapper>(Wrap(json));
            return root.GetList<Player>(abbr) ?? PlaceholderRoster();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LeagueRepository] Roster parse failed: {e.Message}");
            return PlaceholderRoster();
        }
    }
    private static List<TeamGame> ParseSchedule(string json, string abbr)
    {
        try
        {
            var root = JsonUtility.FromJson<Wrapper>(Wrap(json));
            return root.GetList<TeamGame>(abbr) ?? new List<TeamGame>();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LeagueRepository] Schedule parse failed: {e.Message}");
            return new List<TeamGame>();
        }
    }

    // JsonUtility can’t do dicts; wrap into a field name
    [Serializable] private class Wrapper { public string _raw;
        public List<T> GetList<T>(string key)
        {
            // crude extraction: "key":[ ... ] ; assumes well-formed JSON
            var target = $"\"{key}\"";
            int i = _raw.IndexOf(target, StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            int s = _raw.IndexOf('[', i);
            int e = _raw.IndexOf(']', s);
            if (s < 0 || e < 0) return null;
            string arr = _raw.Substring(s, e - s + 1);
            return JsonArray<T>(arr);
        }
    }
    private static string Wrap(string raw) => "{\"_raw\":" + JsonUtility.ToJson(raw) + "}";
    private static List<T> JsonArray<T>(string arr) => new List<T>(JsonHelper.FromJson<T>(arr));

    private static List<Player> PlaceholderRoster()
    {
        var list = new List<Player>();
        for (int i = 0; i < 12; i++) list.Add(new Player { id = Guid.NewGuid().ToString("N"), name = $"Player {i+1}", position = "WR", overall = UnityEngine.Random.Range(60, 85), number = 10 + i });
        return list;
    }
}

// JsonUtility array helper
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string wrapped = "{\"items\":" + json + "}";
        Wrapper<T> w = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return w.items ?? Array.Empty<T>();
    }
    [Serializable] private class Wrapper<T> { public T[] items; }
}
