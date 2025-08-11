using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using GridironGM.Data;

namespace GridironGM.UI.TeamSelection
{
    public class RosterPanelUI : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private Transform content;            // ScrollView Content (RightPanel/Viewport/Content)
        [SerializeField] private GameObject playerRowPrefab;   // Prefab with PlayerRowUI script
        [SerializeField] private TMP_Text messageText;         // Optional "No data" label

        [Header("Dev")]
        [Tooltip("If ON, adds a faint background + fixed height to each row so visibility is obvious.")]
        [SerializeField] private bool forceRowVisible = true;

        private RosterByTeam rosters;

        private void Awake()
        {
            TryAutoWire();
            ReloadRosters(); // load whatever exists at startup
        }

        /// <summary>Re-read rosters_by_team.json into memory.</summary>
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

        /// <summary>Clear and render the selected team's roster.</summary>
        public void ShowRosterForTeam(string abbr)
        {
            if (!content || !playerRowPrefab)
            {
                Debug.LogError("[RosterPanelUI] Missing Content or Player Row Prefab.");
                return;
            }

            // If the file was generated in Start(), make sure we refresh memory
            if (rosters == null || rosters.Count == 0)
                ReloadRosters();

            // Clear old children
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            if (messageText) messageText.text = "";

            if (rosters == null || !rosters.TryGetValue(abbr, out var list) || list == null || list.Count == 0)
            {
                if (messageText) messageText.text = $"No roster data for {abbr}";
                Debug.LogWarning($"[RosterPanelUI] No roster for {abbr}.");
                return;
            }

            int rendered = 0;
            foreach (var p in list.OrderBy(pl => pl.pos).ThenByDescending(pl => pl.ovr))
            {
                var go = Instantiate(playerRowPrefab, content);
                go.name = $"{abbr}_{p.pos}_{p.last}_{p.ovr}";
                if (forceRowVisible) EnsureRowVisible(go);

                var row = go.GetComponent<PlayerRowUI>();
                if (!row)
                {
                    Debug.LogError("[RosterPanelUI] PlayerRowUI missing on prefab. Add the script to the prefab root.");
                    continue;
                }
                var rt = content as RectTransform;
                if (rt)
                {
                    Canvas.ForceUpdateCanvases();
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                }


                row.Set(p);
                rendered++;
            }

            Debug.Log($"[RosterPanelUI] Rendered {rendered}/{list.Count} players for {abbr}. Content children now: {content.childCount}");
        }

        private void EnsureRowVisible(GameObject go)
        {
            // Make sure each row has a background and a reasonable height so it actually shows up.
            var rt = go.GetComponent<RectTransform>();
            if (rt) rt.sizeDelta = new Vector2(rt.sizeDelta.x, 36f);

            var img = go.GetComponent<Image>();
            if (!img) img = go.AddComponent<Image>();
            img.raycastTarget = false;
            // faint background so you can see rows are there
            var c = img.color;
            img.color = new Color(c.r == 0 ? 0.1f : c.r, c.g == 0 ? 0.1f : c.g, c.b == 0 ? 0.1f : c.b, 0.08f);

            // Help layout systems if missing
            var le = go.GetComponent<LayoutElement>();
            if (!le) le = go.AddComponent<LayoutElement>();
            le.minHeight = 32f;
            le.preferredHeight = 36f;
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

