using UnityEngine;

public class SchedulePanelView : MonoBehaviour, ITabView
{
    public void OnTabShown()
    {
        // GetComponent<ScheduleBinder>()?.Refresh();
        Debug.Log("[SchedulePanelView] Refresh on show");
    }
}
