using UnityEngine;

namespace GridironGM
{
    // Simple global state that auto-creates itself if needed and persists across scenes.
    public class GameState : MonoBehaviour
    {
        private static GameState _instance;
        public static GameState Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("GameState");
                    _instance = go.AddComponent<GameState>();
                }
                return _instance;
            }
        }

        public string SelectedTeamAbbr { get; set; } = "";
        public string SelectedTeamCity { get; set; } = "";
        public string SelectedTeamName { get; set; } = "";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
