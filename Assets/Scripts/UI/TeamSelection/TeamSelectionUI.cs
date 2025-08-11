using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GridironGM;
using GridironGM.Data;

namespace GridironGM.UI
{
    public class TeamSelectionUI : MonoBehaviour
    {
        [SerializeField] private Transform teamListContent;
        [SerializeField] private TeamRowUI teamRowPrefab;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private RosterPanelUI rosterPanel;

        private void Start()
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }
            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }

            List<TeamData> teams = JsonLoader.LoadFromStreamingAssets<List<TeamData>>("teams.json");
            if (teams == null || teams.Count == 0)
            {
                Debug.LogError("teams.json missing or empty");
                ShowError("Failed to load team data");
                return;
            }

            Debug.Log($"Loaded {teams.Count} teams");

            foreach (Transform child in teamListContent)
            {
                Destroy(child.gameObject);
            }

            foreach (TeamData team in teams.OrderBy(t => t.city).ThenBy(t => t.name))
            {
                TeamRowUI row = Instantiate(teamRowPrefab, teamListContent);
                row.Set(team, () => OnTeamClicked(team));
            }
        }

        private void OnTeamClicked(TeamData team)
        {
            GameState.Instance.SelectedTeamAbbr = team.abbreviation;
            Debug.Log($"Team clicked: {team.abbreviation}");
            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }
            rosterPanel?.ShowRosterForTeam(team.abbreviation);
        }

        public void OnConfirm()
        {
            SceneManager.LoadScene("NewGameSetup");
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }
        }
    }
}
