using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace GridironGM.UI.TeamSelection
{
    public class TeamSelectionUI : MonoBehaviour
    {
        [Header("Left list")]
        [SerializeField] private Transform teamListContent;   // LeftPanel/Viewport/Content
        [SerializeField] private GameObject teamRowPrefab;    // TeamRowUI prefab

        [Header("Right panel")]
        [SerializeField] private RosterPanelUI rosterPanel;   // RightPanel has this

        [Header("Flow")]
        [SerializeField] private Button confirmButton;        // bottom confirm button (optional)

        private List<GridironGM.Data.TeamData> teams;

        private void Awake()
        {
            // If confirm button is wired, start disabled
            if (confirmButton) confirmButton.interactable = false;
        }

        [System.Obsolete]
        private void Start()
        {
            GridironGM.Boot.RosterBootstrapper.EnsureRostersExist(playersPerTeam: 12);

            TryAutoWire();

            teams = GridironGM.Data.JsonLoader
                .LoadFromStreamingAssets<List<GridironGM.Data.TeamData>>("teams.json");

            if (teams == null || teams.Count == 0)
            {
                Debug.LogError("[TeamSelectionUI] teams.json missing or empty.");
                return;
            }

            foreach (var t in teams.OrderBy(t => (t.city + " " + t.name)))
                AddTeamRow(t);

            Debug.Log($"[TeamSelectionUI] Spawned {teams.Count} teams.");
        }

        private void AddTeamRow(GridironGM.Data.TeamData t)
        {
            if (!teamListContent || !teamRowPrefab)
            {
                Debug.LogError("[TeamSelectionUI] teamListContent or teamRowPrefab not assigned.");
                return;
            }

            var go  = Instantiate(teamRowPrefab, teamListContent);
            var row = go.GetComponent<TeamRowUI>();
            if (!row)
            {
                Debug.LogError("[TeamSelectionUI] TeamRowUI missing on teamRowPrefab.");
                return;
            }

            row.Set(t, () => OnTeamClicked(t));
        }

        private void OnTeamClicked(GridironGM.Data.TeamData team)
        {
            if (team == null) { Debug.LogError("[TeamSelectionUI] Null team"); return; }

            // Save selection if GameState exists
            if (GridironGM.GameState.Instance != null)
            {
                GridironGM.GameState.Instance.SelectedTeamAbbr = team.abbreviation;
            }
            else
            {
                Debug.LogWarning("[TeamSelectionUI] GameState.Instance is null; selection not persisted.");
            }

            // Enable confirm button if wired
            if (confirmButton) confirmButton.interactable = true;

            // Show roster on right if wired
            if (rosterPanel) rosterPanel.ShowRosterForTeam(team.abbreviation);

            Debug.Log($"[TeamSelectionUI] Selected {team.abbreviation}");
        }

        public void OnConfirm()
        {
            var abbr = GridironGM.GameState.Instance?.SelectedTeamAbbr;
            if (string.IsNullOrEmpty(abbr))
            {
                Debug.LogWarning("[TeamSelectionUI] No team selected.");
                return;
            }

            SceneManager.LoadScene("NewGameSetup"); // or whatever your next scene is
        }

        [System.Obsolete]
        private void TryAutoWire()
        {
            if (!teamListContent)
            {
                var tf = transform.root.Find("Canvas/LeftPanel/Viewport/Content");
                if (!tf) tf = GameObject.Find("LeftPanel/Viewport/Content")?.transform;
                teamListContent = tf;
            }
            if (!rosterPanel)
                rosterPanel = FindObjectOfType<RosterPanelUI>(true);
        }
    }
}
