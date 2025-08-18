using System.Linq;
using TMPro;
using UnityEngine;

public class ScheduleBinder : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject textRowPrefab; // simple row with a TMP_Text
    [SerializeField] private TMP_Text headerText;

    public void Refresh()
    {
        if (contentParent == null || textRowPrefab == null) { Debug.LogWarning("[ScheduleBinder] Missing refs"); return; }
        for (int i = contentParent.childCount - 1; i >= 0; i--) Destroy(contentParent.GetChild(i).gameObject);

        string abbr = GameSession.SelectedTeamAbbr;
        var games = LeagueRepository.GetTeamSchedule(abbr);
        headerText?.SetText($"{abbr} • Schedule");
        int curWeek = GameSession.CurrentWeek;

        foreach (var g in games.OrderBy(g => g.week))
        {
            var row = Instantiate(textRowPrefab, contentParent);
            var txt = row.GetComponentInChildren<TMP_Text>();
            string vs = g.home ? "vs" : "@";
            string res = string.IsNullOrEmpty(g.result) ? "" : $"  •  {g.result}";
            txt.SetText($"Week {g.week}: {vs} {g.opponent}{res}");

            // highlight current week
            if (g.week == curWeek)
            {
                var cg = row.GetComponent<CanvasGroup>() ?? row.AddComponent<CanvasGroup>();
                cg.alpha = 1f;
                row.transform.localScale = Vector3.one * 1.02f;
            }
        }

        Canvas.ForceUpdateCanvases();
    }
}
