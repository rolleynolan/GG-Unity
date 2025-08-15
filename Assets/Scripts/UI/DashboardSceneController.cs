using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GG.Game
{
    // Runs late so it wins over any legacy "defaults"
    [DefaultExecutionOrder(10000)]
    public class DashboardSceneController : MonoBehaviour
    {
        [Header("Assign these once in the Dashboard scene")]
        [SerializeField] TMP_Text titleLine1;   // top line (City)
        [SerializeField] TMP_Text titleLine2;   // bottom line ("Name (ABBR)")
        [SerializeField] Image    teamLogo;     // crest/logo (Image)

        Dictionary<string, TeamData> _teams;

        void Awake()
        {
            // Refuse to run outside the Dashboard scene (prevents accidents in other scenes)
            if (gameObject.scene.name != "Dashboard")
            {
                enabled = false;       // prevents Start()
                return;
            }
        }

        void Start()
        {
            var abbr = ResolveAbbr();
            TryRebindHeaderIfBad();
            var team = GetTeam(abbr);

            // Header text
            if (titleLine1) titleLine1.text = team != null ? team.city : abbr;
            if (titleLine2) titleLine2.text = team != null ? $"{team.name} ({abbr})" : abbr;

            // Logo
            if (teamLogo)
            {
                var spr = LogoService.Get(abbr);
                teamLogo.enabled = spr != null;
                teamLogo.sprite  = spr;
                teamLogo.preserveAspect = true;
            }

            // Roster panel (if present)
            var panel = FindFirstObjectByType<RosterPanelUI>(FindObjectsInactive.Include);
            if (panel) panel.ShowRosterForTeam(abbr);

            Debug.Log($"[DashboardSceneController] Applied selected team '{abbr}' to header & roster.");
        }

        bool LooksLikeTabLabel(TMP_Text t)
        {
            if (!t) return false;
            string n = t.name.ToLowerInvariant();
            string txt = (t.text ?? "").Trim().ToLowerInvariant();
            if (t.GetComponentInParent<UnityEngine.UI.Selectable>() != null) return true;
            return
                n.Contains("tab") || n.Contains("button") ||
                n.Contains("roster") || n.Contains("depth") || n.Contains("schedule") ||
                txt == "roster" || txt == "depth charts" || txt == "team schedule";
        }

        void TryRebindHeaderIfBad()
        {
            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (!canvas) return;

            if (LooksLikeTabLabel(titleLine1) || LooksLikeTabLabel(titleLine2) || teamLogo == null)
            {
                // Reuse the installer heuristics (simplified)
                var tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>(true)
                                 .Where(t => ((RectTransform)t.transform).anchoredPosition.y > -200f)
                                 .Where(t => !LooksLikeTabLabel(t))
                                 .OrderByDescending(t => t.fontSize)
                                 .ToList();

                titleLine1 = titleLine1 && !LooksLikeTabLabel(titleLine1) ? titleLine1 : tmps.FirstOrDefault();
                titleLine2 = titleLine2 && !LooksLikeTabLabel(titleLine2) ? titleLine2 : tmps.Skip(1).FirstOrDefault();

                var imgs = canvas.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                teamLogo = teamLogo ? teamLogo :
                           imgs.FirstOrDefault(i =>
                               (i.name.ToLowerInvariant().Contains("logo") || i.name.ToLowerInvariant().Contains("crest")) &&
                               ((RectTransform)i.transform).rect.width >= 16f &&
                               ((RectTransform)i.transform).rect.height >= 16f);
                Debug.Log("[DashboardSceneController] Rebound header fields at runtime.");
            }
        }

        string ResolveAbbr()
        {
            if (!string.IsNullOrEmpty(GameState.SelectedTeamAbbr))
                return GameState.SelectedTeamAbbr;
            var p = PlayerPrefs.GetString("selected_team", "");
            return string.IsNullOrEmpty(p) ? "ATL" : p;
        }

        TeamData GetTeam(string abbr)
        {
            if (_teams == null)
            {
                var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
                var list = new List<TeamData>();
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path).TrimStart();
                    if (json.StartsWith("[")) json = "{\"teams\":" + json + "}";
                    list = JsonUtility.FromJson<TeamDataList>(json)?.teams ?? new List<TeamData>();
                }
                _teams = list.Where(t => !string.IsNullOrEmpty(t.abbreviation))
                             .ToDictionary(t => t.abbreviation, t => t, System.StringComparer.OrdinalIgnoreCase);
            }
            _teams.TryGetValue(abbr, out var team);
            return team;
        }
    }
}
