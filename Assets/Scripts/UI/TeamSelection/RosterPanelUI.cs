using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GridironGM.UI.TeamSelection
{
    public class RosterPanelUI : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private Transform content;            // ScrollView Content
        [SerializeField] private GameObject playerRowPrefab;   // Prefab with PlayerRowUI
        [SerializeField] private TMP_Text messageText;         // Optional "No data" label

        [Header("Behavior")]
        [SerializeField] private bool loadSelectedOnEnable = false;

        private Dictionary<string, List<GridironGM.Data.PlayerData>> rosters;

        private void Awake()
        {
            TryAutoWire();
            LoadRostersOnce();
        }

        private void OnEnable()
        {
            if (loadSelectedOnEnable && GridironGM.GameState.Instance != null)
            {
                var abbr = GridironGM.GameState.Instance.SelectedTeamAbbr;
                if (!string.IsNullOrEmpty(abbr))
                    ShowRosterForTeam(abbr);
            }
        }

        public void ShowRosterForTeam(string abbr)
        {
            if (content == null || playerRowPrefab == null)
            {
                Debug.LogError("[RosterPanelUI] Missing content or playerRowPrefab.");
                return;
            }

            // Clear old rows
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            if (rosters == null || !rosters.TryGetValue(abbr, out var list) || list == null || list.Count == 0)
            {
                if (messageText) messageText.text = $"No roster data for {abbr}";
                return;
            }

            if (messageText) messageText.text = "";

            foreach (var p in list
                .OrderBy(pl => pl.pos)
                .ThenByDescending(pl => pl.ovr))
            {
                var go = Instantiate(playerRowPrefab, content);
                var row = go.GetComponent<PlayerRowUI>();
                if (!row)
                {
                    Debug.LogError("[RosterPanelUI] PlayerRowUI missing on prefab.");
                    continue;
                }
                row.Set(p);
            }

            Debug.Log($"[RosterPanelUI] Rendered {list.Count} players for {abbr}.");
        }

        private void LoadRostersOnce()
        {
            if (rosters != null) return;

            try
            {
                // Expecting GridironGM.Data.RosterByTeam : Dictionary<string, List<PlayerData>>
                rosters = GridironGM.Data.JsonLoader.LoadFromStreamingAssets<GridironGM.Data.RosterByTeam>("rosters_by_team.json");
                if (rosters == null)
                    Debug.LogWarning("[RosterPanelUI] rosters_by_team.json not found or empty.");
                else
                    Debug.Log($"[RosterPanelUI] Loaded roster map for {rosters.Count} teams.");
            }
            catch (System.SystemException ex)
            {
                Debug.LogError($"[RosterPanelUI] Failed to load rosters: {ex.Message}");
            }
        }

        private void TryAutoWire()
        {
            if (!content)
            {
                var tf = transform.Find("Viewport/Content") ?? transform.Find("Content");
                if (tf) content = tf;
            }
            if (!messageText)
            {
                var msg = transform.Find("MessageText")?.GetComponent<TMP_Text>();
                if (msg) messageText = msg;
            }
        }
    }
}
