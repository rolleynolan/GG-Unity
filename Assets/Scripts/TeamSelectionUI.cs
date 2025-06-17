using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TeamSelectionUI : MonoBehaviour
{
    private string selectedTeam = "";

    public Button confirmButton;

    private void Start()
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
    }

    public void OnTeamButtonPressed(string teamAbbreviation)
    {
        selectedTeam = teamAbbreviation;
        Debug.Log("Selected Team: " + selectedTeam);
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }
    }

    public void OnConfirmPressed()
    {
        if (string.IsNullOrEmpty(selectedTeam))
        {
            Debug.Log("No team selected.");
            return;
        }

        PlayerPrefs.SetString("selected_team", selectedTeam);
        SceneManager.LoadScene("GameWorld");
    }

    public void OnBackPressed()
    {
        SceneManager.LoadScene("NewGameSetup");
    }
}
