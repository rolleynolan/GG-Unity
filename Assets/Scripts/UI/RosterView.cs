using GG.Game;
using UnityEngine;

public class RosterView : MonoBehaviour
{
    [SerializeField] private Transform contentParent;   // assign Content under ScrollRect
    [SerializeField] private GameObject playerRowPrefab; // assign PlayerRowUI prefab

    void OnEnable()
    {
        if (!contentParent || !playerRowPrefab)
        {
            Debug.LogWarning("[RosterPanel] Missing Content or Row Prefab refs.");
            return;
        }

        var abbr = GameState.SelectedTeamAbbr;
        if (string.IsNullOrEmpty(abbr))
        {
            Debug.LogError("[RosterPanel] SelectedTeamAbbr is empty.");
            return;
        }

        var roster = RosterService.LoadRosterFor(abbr);
        if (roster?.players == null)
        {
            Debug.LogError($"[RosterPanel] No roster found for {abbr}");
            return;
        }

        // Clear any leftover editor placeholders (e.g., "New Text")
        int before = contentParent.childCount;
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Object.Destroy(contentParent.GetChild(i).gameObject);
        Debug.Log($"[RosterPanel] Cleared {before} children under {contentParent.name}");

        int count = 0;
        foreach (var p in roster.players)
        {
            var row = Object.Instantiate(playerRowPrefab, contentParent);
            var binder = row.GetComponent<PlayerRowBinder>();
            if (!binder)
            {
                Debug.LogWarning("[RosterPanel] PlayerRowUI missing PlayerRowBinder; adding and auto-wiring.");
                binder = row.AddComponent<PlayerRowBinder>();
            }
            binder.AutoWireIfNeeded();
            binder.Bind(p);
            count++;
        }

        Debug.Log($"[RosterPanelUI] Shown roster for {abbr} ({count} players)");
    }
}
