using UnityEngine;

namespace GridironGM
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance;

        public string SelectedTeamAbbr = "";

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
