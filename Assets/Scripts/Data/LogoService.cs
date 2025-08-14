using System;
using System.Collections.Generic;
using UnityEngine;

public static class LogoService
{
    private static Dictionary<string, Sprite> _map;

    private static void EnsureLoaded()
    {
        if (_map != null) return;
        _map = new(StringComparer.OrdinalIgnoreCase);
        var db = Resources.Load<TeamLogoDatabase>("Data/TeamLogoDatabase");
        if (!db)
        {
            Debug.LogWarning("[LogoService] TeamLogoDatabase not found at Resources/Data/TeamLogoDatabase");
            return;
        }
        foreach (var e in db.entries)
            if (e.sprite && !string.IsNullOrEmpty(e.abbreviation))
                _map[e.abbreviation] = e.sprite;
        Debug.Log($"[LogoService] Loaded {_map.Count} team logos.");
    }

    public static Sprite Get(string abbr)
    {
        EnsureLoaded();
        return (_map != null && _map.TryGetValue(abbr, out var s)) ? s : null;
    }
}

