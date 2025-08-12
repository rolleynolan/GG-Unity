using UnityEngine;

public class RosterPanelView : MonoBehaviour, ITabView
{
    public void OnTabShown()
    {
        GetComponent<RosterBinder>()?.Refresh();
    }
}
