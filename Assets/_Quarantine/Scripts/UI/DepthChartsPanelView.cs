using UnityEngine;

public class DepthChartsPanelView : MonoBehaviour, ITabView
{
    public void OnTabShown()
    {
        GetComponent<DepthChartsBinder>()?.Refresh();
    }
}
