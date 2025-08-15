using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DashboardHeaderBinder : MonoBehaviour
{
    [Header("Optional explicit refs (auto-wires if left empty)")]
    [SerializeField] TMP_Text teamTitle;     // e.g., "Baltimore Knights (BAL)"
    [SerializeField] Image    teamLogo;      // crest/logo

    Dictionary<string, TeamData> _teams;

    public void Apply(string abbr)
    {
        abbr = (abbr ?? "").ToUpperInvariant();
        EnsureTeamIndex();

        // Auto-wire if not set
        if (!teamTitle) teamTitle = FindBestTitleText();
        if (!teamLogo)  teamLogo  = FindBestLogoImage();

        // Safe lookup (no ?. with out var)
        TeamData t = null;
        if (_teams != null && _teams.TryGetValue(abbr, out var found))
            t = found;

        if (teamTitle)
            teamTitle.text = (t != null) ? $"{t.city} {t.name} ({abbr})" : abbr;

        if (teamLogo)
        {
            var spr = LogoService.Get(abbr);
            teamLogo.enabled     = spr != null;
            teamLogo.sprite      = spr;
            teamLogo.preserveAspect = true;
        }

        Debug.Log($"[HeaderBinder] Applied header for {abbr}");
    }

    // -------- helpers --------

    void EnsureTeamIndex()
    {
        if (_teams != null) return;
        var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
        var list = new List<TeamData>();
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path).TrimStart();
            if (json.StartsWith("[")) json = "{\"teams\":" + json + "}";
            list = JsonUtility.FromJson<TeamDataList>(json)?.teams ?? new List<TeamData>();
        }
        _teams = new Dictionary<string, TeamData>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in list)
            if (!string.IsNullOrEmpty(t.abbreviation))
                _teams[t.abbreviation] = t;
    }

    TMP_Text FindBestTitleText()
    {
        // Prefer names containing Team/Title/Header; else largest TMP under this root.
        var tmps = GetComponentsInChildren<TMP_Text>(true);
        var byName = tmps.FirstOrDefault(x =>
             x.name.IndexOf("Team", StringComparison.OrdinalIgnoreCase) >= 0 ||
             x.name.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0 ||
             x.name.IndexOf("Header", StringComparison.OrdinalIgnoreCase) >= 0);
        if (byName) return byName;
        return tmps.OrderByDescending(x => x.fontSize).FirstOrDefault();
    }

    Image FindBestLogoImage()
    {
        // Pick square-ish Image closest to the title if possible
        var images = GetComponentsInChildren<Image>(true);
        return images
            .OrderBy(img => Mathf.Abs(((RectTransform)img.transform).rect.width - ((RectTransform)img.transform).rect.height))
            .FirstOrDefault();
    }
}

