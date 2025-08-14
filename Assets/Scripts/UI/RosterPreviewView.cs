using UnityEngine;

public class RosterPreviewView : MonoBehaviour
{
    [SerializeField] private Transform contentParent;   // RightPanel/Viewport/Content
    [SerializeField] private GameObject playerRowPrefab; // PlayerRowUI prefab

    public void Show(string abbr)
    {
        if (!contentParent || !playerRowPrefab) { Debug.LogWarning("[RosterPreview] Missing refs"); return; }
        var roster = RosterService.LoadRosterFor(abbr);
        if (roster?.players == null) { Debug.LogWarning($"[RosterPreview] No roster for {abbr}"); return; }

        for (int i = contentParent.childCount - 1; i >= 0; i--) Destroy(contentParent.GetChild(i).gameObject);
        int count = 0;
        foreach (var p in roster.players)
        {
            var row = Instantiate(playerRowPrefab, contentParent);
            var binder = row.GetComponent<PlayerRowBinder>() ?? row.AddComponent<PlayerRowBinder>();
            binder.AutoWireIfNeeded();
            binder.Bind(p);
            count++;
        }
        Debug.Log($"[RosterPreview] Rendered {count} players for {abbr}");
    }
}

