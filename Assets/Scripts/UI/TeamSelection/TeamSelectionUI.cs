using System.Collections.Generic;
using System.Linq;
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
        private TeamRowUI _currentSelectedRow;

        private void Start()
        {
            if (confirmButton != null) confirmButton.interactable = false;

            if (rosterPanel == null)
                rosterPanel = UnityEngine.Object.FindFirstObjectByType<RosterPanelUI>();

            PopulateTeams();
        }

        private void PopulateTeams()
        {
            // Ensure complete rosters, then refresh the panel's memory, THEN build left list
            GridironGM.Boot.RosterBootstrapper.EnsureRostersExist(playersPerTeam: 12);
            if (rosterPanel != null) rosterPanel.ReloadRosters();

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

            var rowGO = Instantiate(teamRowPrefab, teamListContent);
            var row = rowGO.GetComponent<TeamRowUI>();
            if (!row)
            {
                Debug.LogError("[TeamSelectionUI] TeamRowUI missing on teamRowPrefab.");
                return;
            }

            row.Init(t, OnRowClicked);
        }

        private void OnRowClicked(TeamRowUI row)
        {
            if (_currentSelectedRow == row) return;

            if (_currentSelectedRow != null)
                _currentSelectedRow.SetSelected(false);

            _currentSelectedRow = row;
            _currentSelectedRow.SetSelected(true);

            var gs = GridironGM.GameState.Instance;
            if (gs == null)
            {
                Debug.LogError("[TeamSelection] GameState.Instance is null. Ensure a GameState exists in a boot scene and is DontDestroyOnLoad.");
            }
            else
            {
                gs.SelectedTeamAbbr = row.Team.abbreviation;
                gs.SelectedTeamCity = row.Team.city;
                gs.SelectedTeamName = row.Team.name;
            }

            if (rosterPanel != null)
                rosterPanel.ShowRosterForTeam(row.Team.abbreviation);

            if (confirmButton != null)
                confirmButton.interactable = true;

            Debug.Log($"[TeamSelection] Selected {row.Team.abbreviation}");
        }

        public void OnConfirm()
        {
            var gs = GridironGM.GameState.Instance;
            if (gs == null || string.IsNullOrEmpty(gs.SelectedTeamAbbr))
            {
                Debug.LogWarning("[TeamSelectionUI] No team selected.");
                return;
            }

            SceneManager.LoadScene("Dashboard");
        }

        private void TryAutoWire()
        {
            if (!teamListContent)
            {
                var tf = transform.root.Find("Canvas/LeftPanel/Viewport/Content");
                if (!tf) tf = GameObject.Find("LeftPanel/Viewport/Content")?.transform;
                teamListContent = tf;
            }
        }
    }
}
