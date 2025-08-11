using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using GridironGM.Data;

namespace GridironGM.UI.TeamSelection
{
    public class RosterPanelUI : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private Transform content;          // ScrollView Content
        [SerializeField] private GameObject playerRowPrefab; // Prefab with PlayerRowUI
        [SerializeField] private TMP_Text messageText;       // Optional "No data" label

        private RosterByTeam rosters;

        private void Awake()
        {
            TryAutoWire();
            ReloadRosters(); // load whatever exists at startup
        }

        public void ReloadRosters()
        {
            try
            {
                rosters = JsonLoader.LoadFromStreamingAssets<RosterByTeam>("rosters_by_team.json");
                Debug.Log($"[RosterPanelUI] Reloaded roster map for {(rosters?.Count ?? 0)} teams.");
            }
            catch (Exception ex)
            {
                rosters = null;
                Debug.LogError($"[RosterPanelUI] Failed to load rosters: {ex.Message}");
            }
        }

        public void ShowRosterForTeam(string abbr)
        {
            if (!content || !playerRowPrefab)
            {
                Debug.LogError("[RosterPanelUI] Missing Content or Player Row Prefab.");
                return;
            }

            // In case the file was generated after Awake()
            if (rosters == null || rosters.Count == 0)
                ReloadRosters();

            // Clear old rows
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            if (messageText) messageText.text = "";

            if (rosters == null || !rosters.TryGetValue(abbr, out var list) || list == null || list.Count == 0)
            {
                if (messageText) messageText.text = $"No roster data for {abbr}";
                Debug.LogWarning($"[RosterPanelUI] No roster for {abbr}.");
                return;
            }

            foreach (var p in list.OrderBy(pl => pl.pos).ThenByDescending(pl => pl.ovr))
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

        private void TryAutoWire()
        {
            if (!content)
            {
                var sr = GetComponent<ScrollRect>() ?? GetComponentInChildren<ScrollRect>(true);
                if (sr && sr.content) content = sr.content;
                if (!content)
                {
                    var tf = transform.Find("Viewport/Content") ?? transform.Find("ScrollView/Viewport/Content") ?? transform.Find("Content");
                    if (tf) content = tf;
                }
            }
        }
    }
}
