using System.Collections.Generic;
using UnityEngine;

public static class LogoService
{
    private static Dictionary<string, Sprite> _map;
    private const string ResourcePath = "Data/TeamLogoDatabase";

    public static Sprite Get(string abbr)
    {
        if (string.IsNullOrEmpty(abbr)) return null;
        EnsureLoaded();
        _map.TryGetValue(abbr.ToUpperInvariant(), out var s);
        return s;
    }

    private static void EnsureLoaded()
    {
        if (_map != null) return;
        _map = new Dictionary<string, Sprite>();
        var db = Resources.Load<TeamLogoDatabase>(ResourcePath);
        if (!db) { Debug.LogWarning($"[LogoService] Missing Resources/{ResourcePath}.asset"); return; }
        foreach (var e in db.items)
            if (!string.IsNullOrEmpty(e.abbr) && e.sprite)
                _map[e.abbr.ToUpperInvariant()] = e.sprite;
        Debug.Log($"[LogoService] Loaded logos: {_map.Count}");
    }
}
