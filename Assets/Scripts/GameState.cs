using UnityEngine;

namespace GridironGM
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public string SelectedTeamAbbr { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
