using GG.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GG.Game
{
    // Enforces the selected team on Dashboard both as a component and as a static scene hook.
    [DefaultExecutionOrder(1000)]
    public class DashboardBootstrap : MonoBehaviour
    {
        void Start() { ApplySelectionToDashboard(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoadHook()
        {
            if (SceneManager.GetActiveScene().name == "Dashboard")
                ApplySelectionToDashboard();
        }

        static void ApplySelectionToDashboard()
        {
            var abbr = !string.IsNullOrEmpty(GameState.SelectedTeamAbbr)
                       ? GameState.SelectedTeamAbbr
                       : PlayerPrefs.GetString("selected_team", "ATL");

            // Ensure there is a HeaderBinder in the scene; if not, create one at the Canvas root
            var header = Object.FindFirstObjectByType<DashboardHeaderBinder>(FindObjectsInactive.Include);
            if (!header)
            {
                var canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
                if (canvas)
                {
                    var go = new GameObject("HeaderBinder (Runtime)", typeof(DashboardHeaderBinder));
                    go.transform.SetParent(canvas.transform, false);
                    header = go.GetComponent<DashboardHeaderBinder>();
                }
            }
            if (header) header.Apply(abbr);

            // If a RosterPanelUI exists in Dashboard, show that team's roster too
            var panel = Object.FindFirstObjectByType<RosterPanelUI>(FindObjectsInactive.Include);
            if (panel) panel.ShowRosterForTeam(abbr);

            Debug.Log($"[DashboardBootstrap] Enforced selected team {abbr} on Dashboard.");
        }
    }
}

