using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameWorldUI : MonoBehaviour
{
    public Text teamText; // Assign in inspector

    void Start()
    {
        string selectedTeam = PlayerPrefs.GetString("selected_team", "Unknown Team");
        teamText.text = $"Welcome, GM of {selectedTeam}";
    }

    public void OnBackToMenuPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnStartSeasonPressed()
    {
        Debug.Log("Start Season pressed (future sim entry point)");
    }
}
