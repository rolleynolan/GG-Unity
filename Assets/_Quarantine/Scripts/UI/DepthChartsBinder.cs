using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DepthChartsBinder : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject textRowPrefab; // a simple TMP_Text row
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private int slotsPerPosition = 2; // show top N per position

    private static readonly string[] Order = { "QB","RB","WR","TE","LT","LG","C","RG","RT","EDGE","DE","DT","LB","CB","S","K","P" };

    public void Refresh()
    {
        if (contentParent == null || textRowPrefab == null) { Debug.LogWarning("[DepthChartsBinder] Missing refs"); return; }
        for (int i = contentParent.childCount - 1; i >= 0; i--) Destroy(contentParent.GetChild(i).gameObject);

        string abbr = GameSession.SelectedTeamAbbr;
        headerText?.SetText($"{abbr} • Depth Chart");
        var roster = LeagueRepository.GetRoster(abbr);
        if (roster.Count == 0) return;

        var byPos = roster.GroupBy(p => p.position)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.overall).ToList());

        foreach (var pos in Order)
        {
            if (!byPos.TryGetValue(pos, out var list) || list.Count == 0) continue;
            // Pos header
            var h = Instantiate(textRowPrefab, contentParent).GetComponentInChildren<TMP_Text>();
            h.fontStyle = FontStyles.Bold;
            h.SetText(pos);
            // Slots
            for (int i = 0; i < Mathf.Min(slotsPerPosition, list.Count); i++)
            {
                var row = Instantiate(textRowPrefab, contentParent).GetComponentInChildren<TMP_Text>();
                var p = list[i];
                row.SetText($"{i+1}. {p.name}  ({p.overall})");
            }
        }
        Canvas.ForceUpdateCanvases();
    }
}
