using GG.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GG.Game
{
    // Enforce the selected team on Dashboard both via component and static hook.
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

            // Ensure Canvas exists
            var canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (!canvas)
            {
                Debug.LogWarning("[DashboardBootstrap] No Canvas found in Dashboard.");
                return;
            }

            // Find or create a header binder and point it at the whole Canvas
            var header = Object.FindFirstObjectByType<DashboardHeaderBinder>(FindObjectsInactive.Include);
            if (!header)
            {
                var go = new GameObject("HeaderBinder (Runtime)", typeof(DashboardHeaderBinder));
                go.transform.SetParent(canvas.transform, false);
                header = go.GetComponent<DashboardHeaderBinder>();
            }
            if (header) header.GetType().GetField("searchRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(header, canvas.transform);
            header?.Apply(abbr);

            // Drive roster panel too, if present
            var panel = Object.FindFirstObjectByType<RosterPanelUI>(FindObjectsInactive.Include);
            panel?.ShowRosterForTeam(abbr);

            Debug.Log($"[DashboardBootstrap] Enforced selected team {abbr} on Dashboard (canvas='{canvas.name}').");
        }
    }
}

