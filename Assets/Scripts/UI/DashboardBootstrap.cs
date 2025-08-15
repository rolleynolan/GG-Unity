using GG.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GG.Game
{
    // Runs as a component (if present) and ALSO as a global hook after scene load
    [DefaultExecutionOrder(1000)]
    public class DashboardBootstrap : MonoBehaviour
    {
        void Start() { ApplySelectionToDashboard(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoadHook()
        {
            var active = SceneManager.GetActiveScene();
            if (active.name == "Dashboard")
                ApplySelectionToDashboard();
        }

        static void ApplySelectionToDashboard()
        {
            var abbr = !string.IsNullOrEmpty(GameState.SelectedTeamAbbr)
                       ? GameState.SelectedTeamAbbr
                       : PlayerPrefs.GetString("selected_team", "ATL");

            // Header
            var header = Object.FindFirstObjectByType<DashboardHeaderBinder>(FindObjectsInactive.Include);
            if (header) header.Apply(abbr);

            // Roster panel (if present in Dashboard)
            var panel = Object.FindFirstObjectByType<RosterPanelUI>(FindObjectsInactive.Include);
            if (panel) panel.ShowRosterForTeam(abbr);

            Debug.Log($"[DashboardBootstrap] Enforced selected team {abbr} on Dashboard.");
        }
    }
}

