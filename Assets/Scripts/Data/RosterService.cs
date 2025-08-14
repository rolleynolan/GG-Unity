using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable] public class PlayerDTO { public string first_name; public string last_name; public string pos; public int overall; public int age; }
[Serializable] public class TeamRosterDTO { public string abbreviation; public List<PlayerDTO> players = new(); }
[Serializable] public class RostersRoot { public List<TeamRosterDTO> teams = new(); }

public static class RosterService
{
    private static Dictionary<string, TeamRosterDTO> _cache;
    private static bool _loaded;

    public static TeamRosterDTO LoadRosterFor(string abbr)
    {
        EnsureLoaded();
        if (string.IsNullOrEmpty(abbr)) return null;
        _cache.TryGetValue(abbr.ToUpperInvariant(), out var t);
        return t;
    }

    private static void EnsureLoaded()
    {
        if (_loaded) return;
        string path = Path.Combine(Application.streamingAssetsPath, "rosters_by_team.json");
        if (!File.Exists(path)) { Debug.LogError($"[RosterService] Missing {path}"); _cache = new(); _loaded = true; return; }

        string json = File.ReadAllText(path).TrimStart();
        if (json.StartsWith("[")) json = "{\"teams\":" + json + "}";

        RostersRoot root;
        try { root = JsonUtility.FromJson<RostersRoot>(json) ?? new RostersRoot(); }
        catch (Exception e) { Debug.LogError($"[RosterService] Parse error: {e}"); root = new RostersRoot(); }

        _cache = new(StringComparer.OrdinalIgnoreCase);
        foreach (var t in root.teams)
            if (!string.IsNullOrEmpty(t.abbreviation))
                _cache[t.abbreviation] = t;

        Debug.Log($"[RosterService] Loaded rosters for {_cache.Count} teams");
        _loaded = true;
    }
}
