using UnityEngine;

public class RosterPreviewView : MonoBehaviour
{
    [SerializeField] Transform contentParent;
    [SerializeField] GameObject playerRowPrefab;

    public void Show(string abbr)
    {
        if (!contentParent || !playerRowPrefab)
        {
            AutoWire();
            if (!contentParent || !playerRowPrefab)
            {
                Debug.LogWarning("[RosterPreview] Missing Content or PlayerRowUI prefab refs.");
                return;
            }
        }

        var roster = RosterService.LoadRosterFor(abbr);
        if (roster?.players == null) { Debug.LogWarning($"[RosterPreview] No roster for {abbr}"); return; }

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Object.Destroy(contentParent.GetChild(i).gameObject);

        int count = 0;
        foreach (var p in roster.players)
        {
            var row = Object.Instantiate(playerRowPrefab, contentParent);
            var binder = row.GetComponent<PlayerRowBinder>() ?? row.AddComponent<PlayerRowBinder>();
            binder.AutoWireIfNeeded();
            binder.Bind(p);
            count++;
        }
        Debug.Log($"[RosterPreview] Rendered {count} players for {abbr}");
    }

    void AutoWire()
    {
        if (!contentParent)
        {
            var sr = GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);
            if (sr && sr.content) contentParent = sr.content;
        }
        if (!playerRowPrefab)
        {
            // Try to find a prefab named PlayerRowUI anywhere in Resources (optional)
            var go = Resources.Load<GameObject>("UI/PlayerRowUI");
            if (go) playerRowPrefab = go;
        }
    }
}
