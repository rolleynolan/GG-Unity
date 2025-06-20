using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

// This script dynamically loads team data from teams.json, instantiates a
// TeamRowUI prefab for each team, wires up click events, and tracks the
// selected team for the next scene.

[System.Serializable]
public class TeamData
{
    public string city;
    public string name;
    public string conference;
    public string abbreviation;
}

[System.Serializable]
public class TeamDataList
{
    public TeamData[] teams;
}

public class TeamSelectionUI : MonoBehaviour
{
    public GameObject teamRowPrefab;         // Prefab representing a team row
    public Transform contentParent;          // Parent for instantiated rows
    public Button confirmButton;

    [Header("Roster Preview")]
    public GameObject playerRowPrefab;
    public Transform rosterContent;

    private string selectedAbbreviation = "";
    private Dictionary<string, TeamInfo> teamsByAbbrev;

    void Start()
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
        LoadLeagueState();
        PopulateTeams();
    }

    void PopulateTeams()
    {
        Debug.Log("Attempting to load teams.json...");

        if (teamRowPrefab == null || contentParent == null) return;

        TextAsset json = Resources.Load<TextAsset>("teams");
        if (json == null)
        {
            Debug.LogError("teams.json not found in Resources folder.");
            return;
        }
        Debug.Log("teams.json loaded successfully.");
        Debug.Log("JSON Content: " + json.text);
        TeamDataList dataList = JsonUtility.FromJson<TeamDataList>("{\"teams\":" + json.text + "}");

        foreach (var team in dataList.teams)
        {
            GameObject row = Instantiate(teamRowPrefab, contentParent);
            TeamRowUI ui = row.GetComponent<TeamRowUI>();
            if (ui == null) continue;

            Sprite logo = Resources.Load<Sprite>($"teamsprites/{team.abbreviation}");
            TeamDataUI uiData = new TeamDataUI
            {
                teamName = $"{team.city} {team.name}",
                teamConference = team.conference,
                abbreviation = team.abbreviation,
                logo = logo
            };
            ui.SetData(uiData);

            ui.OnRowClicked = () =>
            {
                selectedAbbreviation = team.abbreviation;
                PlayerPrefs.SetString("selected_team", selectedAbbreviation);
                confirmButton.interactable = true;
                Debug.Log("Selected Team: " + selectedAbbreviation);
                PopulateRoster(selectedAbbreviation);
            };
        }
    }

    public void OnConfirmPressed()
    {
        if (string.IsNullOrEmpty(selectedAbbreviation)) return;

        PlayerPrefs.SetString("selected_team", selectedAbbreviation);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameWorld");
    }

    public void OnBackPressed()
    {
        SceneManager.LoadScene("NewGameSetup");
    }

    void LoadLeagueState()
    {
        var path = Path.Combine(Application.dataPath, "..", "save", "league_state.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("league_state.json not found at " + path);
            return;
        }
        string json = File.ReadAllText(path);
        LeagueState state = JsonUtility.FromJson<LeagueState>(json);
        teamsByAbbrev = new Dictionary<string, TeamInfo>();
        if (state != null && state.teams != null)
        {
            foreach (var t in state.teams)
            {
                teamsByAbbrev[t.abbreviation] = t;
            }
        }
    }

    void PopulateRoster(string abbreviation)
    {
        if (rosterContent == null || playerRowPrefab == null || teamsByAbbrev == null)
            return;
        foreach (Transform child in rosterContent)
        {
            Destroy(child.gameObject);
        }
        if (teamsByAbbrev.TryGetValue(abbreviation, out var team) && team.roster != null)
        {
            foreach (var player in team.roster)
            {
                var rowObj = Instantiate(playerRowPrefab, rosterContent);
                var row = rowObj.GetComponent<PlayerRow>();
                if (row != null)
                    row.SetData(player);
            }
        }
    }
}
