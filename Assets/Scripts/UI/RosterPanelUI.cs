using UnityEngine;

public class RosterPanelUI : MonoBehaviour
{
    [SerializeField] Transform content;        // Scroll View / Viewport / Content
    [SerializeField] GameObject playerRowPrefab; // PlayerRowUI prefab

    public void ShowRosterForTeam(string abbr)
    {
        if (!EnsureWired()) { Debug.LogWarning("[RosterPanel] Missing refs"); return; }
        var team = RosterService.LoadRosterFor(abbr);
        if (team?.players == null) { Debug.LogWarning($"[RosterPanel] No roster for {abbr}"); return; }

        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        int count = 0;
        foreach (var p in team.players)
        {
            var row = Instantiate(playerRowPrefab, content);
            var binder = row.GetComponent<PlayerRowBinder>() ?? row.AddComponent<PlayerRowBinder>();
            binder.Bind(p);
            count++;
        }
        Debug.Log($"[RosterPanel] Rendered {count} players for {abbr}");
    }

    bool EnsureWired()
    {
        if (!content)
        {
            var sr = GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);
            if (sr && sr.content) content = sr.content;
        }
        if (!playerRowPrefab)
        {
            var res = Resources.Load<GameObject>("UI/PlayerRowUI");
            if (res) playerRowPrefab = res;
        }
        return content && playerRowPrefab;
    }
}
