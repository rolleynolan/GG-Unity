using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GG.Game;

namespace GridironGM.UI
{
    public class GameWorldUI : MonoBehaviour
    {
        [SerializeField] private Text teamText; // Assign in inspector

        private void Start()
        {
            string selectedTeam = GameState.SelectedTeamAbbr;
            if (string.IsNullOrEmpty(selectedTeam))
            {
                selectedTeam = "Unknown Team";
            }
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
}
