using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RosterBinder : MonoBehaviour
{
    [Header("Prefabs & Targets")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject playerRowPrefab;
    [SerializeField] private TMP_Text headerText;

    public void Refresh()
    {
        if (contentParent == null || playerRowPrefab == null) { Debug.LogWarning("[RosterBinder] Missing refs"); return; }
        Clear();
        string abbr = GameSession.SelectedTeamAbbr;
        var roster = LeagueRepository.GetRoster(abbr);
        headerText?.SetText($"{abbr} • {roster.Count} players");

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
            else
            {
                // fallback: find by child names
                TrySet(row, "NameText", p.name);
                TrySet(row, "PosText", p.position);
                TrySet(row, "OvrText", p.overall.ToString());
            }
        }
        LayoutRebuild();
    }

    private void TrySet(GameObject row, string childName, string text)
    {
        var t = row.transform.Find(childName)?.GetComponent<TMP_Text>();
        if (t) t.SetText(text);
    }

    private void Clear()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }
    private void LayoutRebuild()
    {
        Canvas.ForceUpdateCanvases();
        var rt = contentParent as RectTransform;
        if (rt) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        Canvas.ForceUpdateCanvases();
    }
}
