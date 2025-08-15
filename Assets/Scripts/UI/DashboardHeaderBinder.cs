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
    [SerializeField] TMP_Text teamTitle;          // e.g., "Baltimore Knights (BAL)"
    [SerializeField] Image    teamLogo;           // crest/logo

    [Header("Where to search for header UI (defaults to Canvas)")]
    [SerializeField] Transform searchRoot;

    Dictionary<string, TeamData> _teams;

    public void Apply(string abbr)
    {
        abbr = (abbr ?? "").ToUpperInvariant();
        EnsureTeamIndex();

        // Choose search scope: explicit root > parent Canvas > scene root of this object
        var scope = searchRoot
                    ? searchRoot
                    : (GetComponentInParent<Canvas>(true)?.transform ?? transform.root);

        // Auto-wire if not set: scan the whole UI under scope
        if (!teamTitle) teamTitle = FindBestTitleText(scope);
        if (!teamLogo)  teamLogo  = FindBestLogoImage(scope);

        TeamData team = null;
        if (_teams != null && _teams.TryGetValue(abbr, out var found)) team = found;

        if (teamTitle)
            teamTitle.text = (team != null) ? $"{team.city} {team.name} ({abbr})" : abbr;

        if (teamLogo)
        {
            var spr = LogoService.Get(abbr);
            teamLogo.enabled = spr != null;
            teamLogo.sprite  = spr;
            teamLogo.preserveAspect = true;
        }

        int tmpCount = scope.GetComponentsInChildren<TMP_Text>(true).Length;
        int imgCount = scope.GetComponentsInChildren<Image>(true).Length;
        Debug.Log($"[HeaderBinder] Applied header for {abbr} (scope='{scope.name}', TMPs={tmpCount}, Images={imgCount})");
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

    TMP_Text FindBestTitleText(Transform scope)
    {
        var tmps = scope.GetComponentsInChildren<TMP_Text>(true);
        var byName = tmps.FirstOrDefault(x =>
             x.name.IndexOf("Team",   StringComparison.OrdinalIgnoreCase) >= 0 ||
             x.name.IndexOf("Title",  StringComparison.OrdinalIgnoreCase) >= 0 ||
             x.name.IndexOf("Header", StringComparison.OrdinalIgnoreCase) >= 0);
        if (byName) return byName;

        // Fallback: biggest font size
        return tmps.OrderByDescending(x => x.fontSize).FirstOrDefault();
    }

    Image FindBestLogoImage(Transform scope)
    {
        var images = scope.GetComponentsInChildren<Image>(true);
        // Prefer square-ish images (crests)
        return images
            .OrderBy(img => Mathf.Abs(((RectTransform)img.transform).rect.width - ((RectTransform)img.transform).rect.height))
            .FirstOrDefault();
    }
}

