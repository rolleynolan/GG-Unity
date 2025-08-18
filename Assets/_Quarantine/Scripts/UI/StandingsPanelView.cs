using UnityEngine;

public class StandingsPanelView : MonoBehaviour, ITabView
{
    public void OnTabShown()
    {
        // GetComponent<StandingsBinder>()?.Refresh();
        Debug.Log("[StandingsPanelView] Refresh on show");
    }
}
