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

        private List<Team> teams;
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
            TryAutoWire();

            teams = LeagueRepository.GetTeams()?.ToList();

            if (teams == null || teams.Count == 0)
            {
                Debug.LogError("[TeamSelectionUI] No teams found.");
                return;
            }

            foreach (var t in teams.OrderBy(t => (t.city + " " + t.name)))
                AddTeamRow(t);

            Debug.Log($"[TeamSelectionUI] Spawned {teams.Count} teams.");
        }

        private void AddTeamRow(Team team)
        {
            if (!teamListContent || !teamRowPrefab)
            {
                Debug.LogError("[TeamSelectionUI] teamListContent or teamRowPrefab not assigned.");
                return;
            }

            var rowGO = Instantiate(teamRowPrefab, teamListContent);
            var rowUI = rowGO.GetComponent<TeamRowUI>();
            if (rowUI == null)
            {
                Debug.LogError("[TeamSelection] teamRowPrefab is missing TeamRowUI.");
            }
            else
            {
                rowUI.Init(team, OnRowClicked);
            }
        }

        private void OnRowClicked(TeamRowUI row)
        {
            if (_currentSelectedRow == row) return;

            if (_currentSelectedRow != null)
                _currentSelectedRow.SetSelected(false);

            _currentSelectedRow = row;
            _currentSelectedRow.SetSelected(true);

            GameSession.SelectedTeamAbbr = row.Team.abbreviation;

            if (rosterPanel != null)
                rosterPanel.SetTeam(row.Team.abbreviation);

            if (confirmButton != null)
                confirmButton.interactable = true;

            Debug.Log($"[TeamSelection] Selected {row.Team.abbreviation}");
        }

        // Proceed only if a team is selected
        public void OnConfirm()
        {
            if (string.IsNullOrEmpty(GameSession.SelectedTeamAbbr))
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
