using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TeamSelectionUI : MonoBehaviour
{
    public GameObject teamRowPrefab;
    public Transform contentParent;
    public Button confirmButton;

    private string selectedAbbreviation = "";

    private void Start()
    {
        confirmButton.interactable = false;
        PopulateTeams();
    }

    private void PopulateTeams()
    {
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

            Sprite logo = Resources.Load<Sprite>($"teamsprites/{team.abbreviation}");

            TeamDataUI uiData = new TeamDataUI($"{team.city} {team.name}", team.conference, team.abbreviation, logo);
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
        SceneManager.LoadScene("GameWorld");
    }

    public void OnBackPressed()
    {
        SceneManager.LoadScene("NewGameSetup");
    }
}
