using UnityEngine;

namespace GG.Game
{
    // Run after most scripts so we override any default team selection
    [DefaultExecutionOrder(1000)]
    public class DashboardBootstrap : MonoBehaviour
    {
        void Start()
        {
            var panel = FindFirstObjectByType<RosterPanelUI>(FindObjectsInactive.Include);
            if (!panel)
            {
                Debug.LogWarning("[DashboardBootstrap] No RosterPanelUI found in Dashboard.");
                return;
            }

            // Prefer in-memory selection; fall back to PlayerPrefs; default to ATL
            var abbr = !string.IsNullOrEmpty(GameState.SelectedTeamAbbr)
                       ? GameState.SelectedTeamAbbr
                       : PlayerPrefs.GetString("selected_team", "ATL");

            Debug.Log($"[DashboardBootstrap] Showing roster for {abbr}");
            panel.ShowRosterForTeam(abbr);
        }
    }
}

