using System.Linq;
using TMPro;
using UnityEngine;

public class RosterPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contentParent;     // ScrollView/Viewport/Content
    [SerializeField] private GameObject playerRowPrefab;  // Prefab with PlayerRowUI (Name/Pos/Ovr)
    [SerializeField] private TMP_Text headerText;

    private string currentAbbr;

    // Called by TeamSelection when a team is highlighted/selected
    public void SetTeam(string abbr)
    {
        currentAbbr = abbr;
        Refresh();
    }

    public void Refresh()
    {
        if (contentParent == null || playerRowPrefab == null)
        {
            Debug.LogWarning("[RosterPanelUI] Missing Content or Row Prefab refs.");
            return;
        }

        if (string.IsNullOrEmpty(currentAbbr))
            currentAbbr = GameSession.SelectedTeamAbbr;

        // Clear old rows
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        var roster = LeagueRepository.GetRoster(currentAbbr); // List<Player>
        headerText?.SetText($"{currentAbbr} • {roster.Count} players");

        foreach (var p in roster.OrderByDescending(p => p.overall).ThenBy(p => p.position))
        {
            var row = Instantiate(playerRowPrefab, contentParent);
            var ui = row.GetComponent<PlayerRowUI>();
            if (ui != null)
            {
                ui.nameText.SetText(p.name);
                ui.posText.SetText(p.position);
                ui.ovrText.SetText(p.overall.ToString());
            }
        }

        // Layout rebuild so first show isn’t blank
        Canvas.ForceUpdateCanvases();
        var rt = contentParent as RectTransform;
        if (rt) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        Canvas.ForceUpdateCanvases();

        Debug.Log($"[RosterPanelUI] Shown roster for {currentAbbr} ({roster.Count} players)");
    }
}
