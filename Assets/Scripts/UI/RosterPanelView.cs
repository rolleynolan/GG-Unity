using UnityEngine;

public class RosterPanelView : MonoBehaviour, ITabView
{
    // Inject or find your binder script here if you have one
    public void OnTabShown()
    {
        // Call your existing refresh logic, e.g.:
        // GetComponent<RosterBinder>()?.Refresh();
        Debug.Log("[RosterPanelView] Refresh on show");
    }
}
