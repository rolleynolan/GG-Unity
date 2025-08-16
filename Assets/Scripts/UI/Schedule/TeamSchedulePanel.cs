using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace GG.UI.Schedule
{
    public class TeamSchedulePanel : MonoBehaviour
    {
        [SerializeField] TMP_Text content;
        [SerializeField] string TeamAbbr = "ATL";

        void OnEnable()
        {
            if (!content)
            {
                var go = new GameObject("TeamScheduleText", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                var t = go.AddComponent<TextMeshProUGUI>();
                t.textWrappingMode = TextWrappingModes.NoWrap;
                t.fontSize = 18;
                content = t;
            }
            RefreshTeamSchedule();
        }

        public void RefreshTeamSchedule()
        {
            if (ScheduleRepository.Current == null) { content.text = "No schedule."; return; }
            var sb = new StringBuilder();
            foreach (var w in ScheduleRepository.Current.weeks)
            {
                var games = w.games.Where(g => g.home == TeamAbbr || g.away == TeamAbbr).ToList();
                foreach (var g in games)
                {
                    var vs = (g.home == TeamAbbr) ? $"vs {g.away}" : $"@ {g.home}";
                    var score = g.played ? $"{g.homePts}-{g.awayPts}" : "—";
                    sb.AppendLine($"W{w.week} {vs}   {score}");
                }
            }
            content.text = sb.Length == 0 ? "No games found." : sb.ToString();
        }
    }
}
