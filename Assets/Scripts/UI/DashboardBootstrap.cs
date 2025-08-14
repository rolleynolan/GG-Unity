using GG.Game;
using UnityEngine;

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

        var abbr = string.IsNullOrEmpty(GameState.SelectedTeamAbbr) ? "ATL" : GameState.SelectedTeamAbbr;
        Debug.Log($"[DashboardBootstrap] Showing roster for {abbr}");
        panel.ShowRosterForTeam(abbr);
    }
}
