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
    [SerializeField] Image    teamLogo;      // small crest/logo

    Dictionary<string, TeamData> _teams;

    public void Apply(string abbr)
    {
        abbr = (abbr ?? "").ToUpperInvariant();
        EnsureTeamIndex();

        // Find team data by abbr; if missing, display abbr only
        TeamData t = null;
        _teams?.TryGetValue(abbr, out t);

        // Auto-wire if not set
        if (!teamTitle) teamTitle = FindBestTitleText();
        if (!teamLogo)  teamLogo  = FindBestLogoImage();

        if (teamTitle)
            teamTitle.text = t != null ? $"{t.city} {t.name} ({abbr})" : abbr;

        if (teamLogo)
        {
            var spr = LogoService.Get(abbr);
            teamLogo.enabled = spr != null;
            teamLogo.sprite  = spr;
            if (teamLogo) teamLogo.preserveAspect = true;
        }

        Debug.Log($"[HeaderBinder] Applied header for {abbr}");
    }

    // -------- helpers --------

    void EnsureTeamIndex()
    {
        if (_teams != null) return;

        var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
        if (!File.Exists(path)) { _teams = new(); return; }

        var json = File.ReadAllText(path).TrimStart();
        if (json.StartsWith("[")) json = "{\"teams\":" + json + "}";
        var list = JsonUtility.FromJson<TeamDataList>(json)?.teams ?? new List<TeamData>();

        _teams = new Dictionary<string, TeamData>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in list)
            if (!string.IsNullOrEmpty(t.abbreviation))
                _teams[t.abbreviation] = t;
    }

    TMP_Text FindBestTitleText()
    {
        // Prefer objects named like "TeamName", "Title", "HeaderTitle"
        var cands = GetComponentsInChildren<TMP_Text>(true);
        return cands.FirstOrDefault(x =>
                 x.name.IndexOf("Team", StringComparison.OrdinalIgnoreCase) >= 0
              || x.name.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0
              || x.name.IndexOf("Header", StringComparison.OrdinalIgnoreCase) >= 0)
            ?? cands.FirstOrDefault();
    }

    Image FindBestLogoImage()
    {
        // Prefer images near the header object with small square rects
        var imgs = GetComponentsInChildren<Image>(true);
        return imgs.OrderBy(i => Mathf.Abs(((RectTransform)i.transform).rect.width - ((RectTransform)i.transform).rect.height))
                   .FirstOrDefault();
    }
}

