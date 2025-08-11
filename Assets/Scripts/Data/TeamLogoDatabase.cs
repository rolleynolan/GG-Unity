using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeamLogoEntry { public string abbreviation; public Sprite sprite; }

public class TeamLogoDatabase : ScriptableObject
{
    [SerializeField] private List<TeamLogoEntry> entries = new();
    private Dictionary<string, Sprite> map;

    public int Count => entries?.Count ?? 0;

    private static TeamLogoDatabase _instance;
    public static TeamLogoDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<TeamLogoDatabase>("TeamLogoDB"); // Assets/Resources/TeamLogoDB.asset
                if (_instance == null) Debug.LogError("[LogoDB] TeamLogoDB.asset NOT found in Resources.");
                else Debug.Log($"[LogoDB] Loaded DB with {_instance.Count} entries.");
            }
            return _instance;
        }
    }

    private static string Norm(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim().Replace("_", "").Replace("-", "").Replace(" ", "");
        return s.ToUpperInvariant();
    }

    private void EnsureMap()
    {
        if (map != null) return;
        map = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in entries)
        {
            if (e?.sprite == null) continue;
            var key = Norm(e.abbreviation);
            if (string.IsNullOrEmpty(key)) continue;
            map[key] = e.sprite;
        }
    }

    public Sprite Get(string abbreviation)
    {
        EnsureMap();
        var key = Norm(abbreviation);
        if (string.IsNullOrEmpty(key)) return null;

        if (map.TryGetValue(key, out var s)) return s;

        // Optional fallback if you later drop copies into Assets/Resources/TeamLogos/ATL.png
        var fallback = Resources.Load<Sprite>($"TeamLogos/{key}");
        if (fallback != null)
        {
            Debug.LogWarning($"[LogoDB] Fallback hit for '{key}' via Resources/TeamLogos.");
            return fallback;
        }

        Debug.LogWarning($"[LogoDB] No logo for '{key}'.");
        return null;
    }

#if UNITY_EDITOR
    public void SetEntries(List<TeamLogoEntry> newEntries)
    {
        entries = newEntries ?? new();
        map = null;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
