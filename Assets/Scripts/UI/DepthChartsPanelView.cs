using UnityEngine;

public class DepthChartsPanelView : MonoBehaviour, ITabView
{
    public void OnTabShown()
    {
        // If you have a binder, call it here:
        // GetComponent<DepthChartsBinder>()?.Refresh();
        Debug.Log("[DepthChartsPanelView] Refresh on show");
    }
}
