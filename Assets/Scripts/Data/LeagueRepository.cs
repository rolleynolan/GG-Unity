using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class LeagueRepository
{
    // Tries persistent path first (runtime saves), falls back to Resources/Config
    private static string PersistPath => Application.persistentDataPath;

    public static Team[] GetTeams()
    {
        if (TryLoadText("teams.json", out var json)) return JsonUtility.FromJson<TeamList>(json)?.teams ?? Array.Empty<Team>();
        if (TryLoadResource("Config/teams", out var resJson)) return JsonUtility.FromJson<TeamList>(resJson)?.teams ?? Array.Empty<Team>();
        Debug.LogWarning("[LeagueRepository] teams.json not found.");
        return Array.Empty<Team>();
    }

    public static List<Player> GetRoster(string abbr)
    {
        if (string.IsNullOrEmpty(abbr)) return new List<Player>();
        if (TryLoadText("rosters_by_team.json", out var json)) return ParseRoster(json, abbr);
        if (TryLoadResource("Generated/rosters_by_team", out var resJson)) return ParseRoster(resJson, abbr);
        Debug.LogWarning("[LeagueRepository] rosters_by_team.json not found. Returning placeholder roster.");
        return PlaceholderRoster();
    }

    public static List<TeamGame> GetTeamSchedule(string abbr)
    {
        if (string.IsNullOrEmpty(abbr)) return new List<TeamGame>();
        if (TryLoadText("schedule_by_team.json", out var json)) return ParseSchedule(json, abbr);
        if (TryLoadResource("Generated/schedule_by_team", out var resJson)) return ParseSchedule(resJson, abbr);
        Debug.LogWarning("[LeagueRepository] schedule_by_team.json not found.");
        return new List<TeamGame>();
    }

    // ===== helpers =====
    private static bool TryLoadText(string fileName, out string json)
    {
        json = null;
        var path = Path.Combine(PersistPath, fileName);
        if (!File.Exists(path)) return false;
        json = File.ReadAllText(path);
        return !string.IsNullOrEmpty(json);
    }
    private static bool TryLoadResource(string resPath, out string json)
    {
        json = null;
        var ta = Resources.Load<TextAsset>(resPath);
        if (ta == null) return false;
        json = ta.text;
        return !string.IsNullOrEmpty(json);
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
