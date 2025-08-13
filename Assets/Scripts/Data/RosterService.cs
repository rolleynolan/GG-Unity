using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class RosterService
{
    private static Dictionary<string, TeamRosterDTO> _cache;
    private static bool _loaded;

    public static TeamRosterDTO LoadRosterFor(string abbr)
    {
        EnsureLoaded();
        if (_cache != null && _cache.TryGetValue(abbr, out var team)) return team;
        Debug.LogError($"[RosterService] Roster not found for {abbr}. Known teams: {(_cache?.Count ?? 0)}");
        return null;
    }

    private static void EnsureLoaded()
    {
        if (_loaded) return;
        string path = Path.Combine(Application.streamingAssetsPath, "rosters_by_team.json");
        if (!File.Exists(path)) { Debug.LogError($"[RosterService] Missing file: {path}"); _cache = new(); _loaded = true; return; }
        string json = File.ReadAllText(path);
        if (json.TrimStart().StartsWith("[")) json = "{\"teams\":" + json + "}";
        RostersRoot root;
        try { root = JsonUtility.FromJson<RostersRoot>(json); }
        catch (Exception e) { Debug.LogError($"[RosterService] Parse error: {e}"); _cache = new(); _loaded = true; return; }
        _cache = new(StringComparer.OrdinalIgnoreCase);
        if (root?.teams != null)
            foreach (var t in root.teams)
                if (!string.IsNullOrEmpty(t.abbreviation)) _cache[t.abbreviation] = t;
        Debug.Log($"[RosterService] Loaded rosters for {_cache.Count} teams from rosters_by_team.json");
        _loaded = true;
    }
}
