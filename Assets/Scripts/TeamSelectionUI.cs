using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TeamData
{
    public string abbreviation;
    public string teamName;
    public string conference;
    public Sprite logo;
}

public class TeamSelectionUI : MonoBehaviour
{
    public GameObject teamRowPrefab;         // Assign the prefab from UI
    public Transform teamRowParent;          // Assign the Content object from ScrollView
    public Button confirmButton;

    public List<TeamData> allTeams;          // Populate in Inspector or from JSON later

    private string selectedTeam = "";

    void Start()
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
        GenerateTeamRows();
    }

    void GenerateTeamRows()
    {
        if (teamRowPrefab == null || teamRowParent == null) return;

        foreach (var team in allTeams)
        {
            GameObject row = Instantiate(teamRowPrefab, teamRowParent);
            TeamRowUI rowUI = row.GetComponent<TeamRowUI>();
            if (rowUI != null)
            {
                rowUI.SetData(team);
                rowUI.OnRowClicked = () => OnTeamSelected(team);
            }
        }
    }

    void OnTeamSelected(TeamData team)
    {
        selectedTeam = team.abbreviation;
        Debug.Log("Selected Team: " + selectedTeam);
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }
    }

    public void OnConfirmPressed()
    {
        if (string.IsNullOrEmpty(selectedTeam)) return;

        PlayerPrefs.SetString("selected_team", selectedTeam);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameWorld");
    }

    public void OnBackPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("NewGameSetup");
    }
}
