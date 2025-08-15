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

        void Start()
        {
            var abbr = ResolveAbbr();
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
