using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GridironGM;

namespace GridironGM.UI
{
    public class GameWorldUI : MonoBehaviour
    {
        [SerializeField] private Text teamText; // Assign in inspector

        private void Start()
        {
            string selectedTeam = GameState.Instance.SelectedTeamAbbr;
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
