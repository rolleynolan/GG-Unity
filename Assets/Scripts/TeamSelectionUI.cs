using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// This script dynamically loads team data from teams.json, instantiates a
// TeamRowUI prefab for each team, wires up click events, and tracks the
// selected team for the next scene.

[System.Serializable]
public class TeamJson
{
    public string city;
    public string name;
    public string abbreviation;
    public string conference;
}

[System.Serializable]
public class TeamDataList
{
    public List<TeamJson> teams;
}

[System.Serializable]
public class TeamData
{
    public string teamName;
    public string teamConference;
    public string abbreviation;
    public Sprite logo;
}

public class TeamSelectionUI : MonoBehaviour
{
    public GameObject teamRowPrefab;         // Prefab representing a team row
    public Transform contentParent;          // Parent for instantiated rows
    public Button confirmButton;

    private string selectedAbbreviation = "";

    void Start()
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
        PopulateTeams();
    }

    void PopulateTeams()
    {
        if (teamRowPrefab == null || contentParent == null) return;

        TextAsset json = Resources.Load<TextAsset>("teams");
        if (json == null)
        {
            Debug.LogError("teams.json not found in Resources folder.");
            return;
        }

        TeamDataList dataList = JsonUtility.FromJson<TeamDataList>("{\"teams\":" + json.text + "}");

        foreach (var team in dataList.teams)
        {
            GameObject row = Instantiate(teamRowPrefab, contentParent);
            TeamRowUI ui = row.GetComponent<TeamRowUI>();
            if (ui == null) continue;

            Sprite logo = Resources.Load<Sprite>($"teamsprites/{team.abbreviation}");
            // Map TeamData to TeamRowUI.TeamDataUI
            var uiData = new TeamRowUI.TeamDataUI
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
}
