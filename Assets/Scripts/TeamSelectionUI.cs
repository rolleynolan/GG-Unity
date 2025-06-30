using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


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

[System.Serializable]
public class SelectionPlayerData
{
    public string name;
    public string position;
    public int jersey_number;
    public int overall;
}

public class TeamSelectionUI : MonoBehaviour
{
    public GameObject teamRowPrefab;         // Prefab representing a team row
    public Transform contentParent;          // Parent for instantiated rows
    public Button confirmButton;

    [Header("Roster Preview")]
    public GameObject playerRowPrefab;
    public Transform rosterContent;
    public Transform rosterContentParent;

    private string selectedAbbreviation = "";
    private Dictionary<string, TeamRosterEntry> teamsByAbbrev;
    private Dictionary<string, List<SelectionPlayerData>> rosters;

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
        dataList.teams = dataList.teams.OrderBy(t => t.city + t.name).ToArray();

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
        LeagueState state = JsonUtility.FromJson<LeagueStateWrapper>($"{{\"leagueState\":{json}}}").leagueState;
        teamsByAbbrev = new Dictionary<string, TeamRosterEntry>();
        if (state != null && state.teams != null)
        {
            foreach (var t in state.teams)
            {
                teamsByAbbrev[t.team] = t;
            }
        }
    }

    void LoadRosters()
    {
        string path = Path.Combine(Application.persistentDataPath, "rosters.json");
        if (!File.Exists(path))
        {
            Debug.LogError("rosters.json not found at " + path);
            rosters = null;
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            rosters = JsonConvert.DeserializeObject<Dictionary<string, List<SelectionPlayerData>>>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse rosters.json: " + ex.Message);
            rosters = null;
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
        if (teamsByAbbrev.TryGetValue(abbreviation, out var team) && team.players != null)
        {
            foreach (var player in team.players)
            {
                var rowObj = Instantiate(playerRowPrefab, rosterContent);
                var row = rowObj.GetComponent<PlayerRow>();
                if (row != null)
                    row.SetData(player);
            }
        }
    }

    public void DisplayRosterForTeam(string abbreviation)
    {
        if (rosterContentParent == null || playerRowPrefab == null)
            return;

        foreach (Transform child in rosterContentParent)
        {
            Destroy(child.gameObject);
        }

        if (rosters == null || !rosters.ContainsKey(abbreviation))
        {
            Debug.LogWarning($"Roster for {abbreviation} not found.");
            return;
        }

        foreach (var player in rosters[abbreviation])
        {
            var rowObj = Instantiate(playerRowPrefab, rosterContentParent);

            var nameText = rowObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var posText = rowObj.transform.Find("PosText")?.GetComponent<TextMeshProUGUI>();
            var numText = rowObj.transform.Find("NumText")?.GetComponent<TextMeshProUGUI>();
            var ovrText = rowObj.transform.Find("OvrText")?.GetComponent<TextMeshProUGUI>();

            if (nameText != null) nameText.text = player.name;
            if (posText != null) posText.text = player.position;
            if (numText != null) numText.text = player.jersey_number.ToString();
            if (ovrText != null) ovrText.text = player.overall.ToString();
        }
    }
}
