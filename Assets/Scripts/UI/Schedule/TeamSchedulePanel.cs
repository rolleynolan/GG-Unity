using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using GG.Bridge.Repositories;

namespace GG.UI.Schedule
{
    public class TeamSchedulePanel : MonoBehaviour
    {
        public TMP_Text content;
        public string TeamAbbr = "ATL";

        void OnEnable()
        {
            if (!content)
            {
                var go = new GameObject("TeamScheduleText", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                content = go.AddComponent<TMP_Text>();
            }
            RefreshTeamSchedule();
        }

        // Called by DashboardSceneController after sims
        void RefreshTeamSchedule()
        {
            if (ScheduleRepository.Current == null) { content.text = "No schedule."; return; }
            var sb = new StringBuilder();
            foreach (var w in ScheduleRepository.Current.Weeks)
            {
                var games = w.Games.Where(g => g.Home == TeamAbbr || g.Away == TeamAbbr).ToList();
                foreach (var g in games)
                {
                    var vs = (g.Home == TeamAbbr) ? $"vs {g.Away}" : $"@ {g.Home}";
                    var score = g.Played ? $"{g.Score[g.Home]}-{g.Score[g.Away]}" : "—";
                    sb.AppendLine($"W{w.Week} {vs}   {score}");
                }
            }
            content.text = sb.Length == 0 ? "No games found." : sb.ToString();
        }
    }
}
