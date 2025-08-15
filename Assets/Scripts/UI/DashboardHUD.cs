using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GG.Season;

namespace GG.Game
{
    public class DashboardHUD : MonoBehaviour
    {
        [SerializeField] TMP_Text weekText;
        [SerializeField] TMP_Text nextOpponentText;
        [SerializeField] Button simButton;
        [SerializeField] Button advanceButton;

        SeasonState _season;

        void Start()
        {
            _season = SeasonState.LoadOrCreate(GameState.SelectedTeamAbbr);
            if (simButton) simButton.onClick.AddListener(OnClickSim);
            if (advanceButton) advanceButton.onClick.AddListener(OnClickAdvance);
            Refresh();
        }

        void Refresh()
        {
            if (weekText)
            {
                if (_season.week > _season.scheduleByWeek.Length) weekText.text = "Season Complete";
                else weekText.text = $"Week {_season.week}";
            }

            if (nextOpponentText)
            {
                var next = _season.GetNextGame(GameState.SelectedTeamAbbr);
                if (next == null) nextOpponentText.text = "Next: ---";
                else
                {
                    bool home = next.Value.home == GameState.SelectedTeamAbbr;
                    string opp = home ? next.Value.away : next.Value.home;
                    nextOpponentText.text = $"Next: {opp} ({(home ? "H" : "A")})";
                }
            }
        }

        int LookupOvr(string abbr)
        {
            var roster = RosterService.LoadRosterFor(abbr);
            if (roster == null || roster.players == null || roster.players.Count == 0) return 60;
            return (int)roster.players.Average(p => p.overall);
        }

        public void OnClickSim()
        {
            var next = _season.GetNextGame(GameState.SelectedTeamAbbr);
            if (next == null) return;
            var result = SimpleSim.Sim(next.Value, LookupOvr, next.Value.week);
            _season.ApplyResult(result);
            _season.Save();
            Debug.Log($"[DashboardHUD] Simmed {result.away} at {result.home}: {result.homeScore}-{result.awayScore}");
            Refresh();
        }

        public void OnClickAdvance()
        {
            if (_season.week <= _season.scheduleByWeek.Length) _season.week++;
            _season.Save();
            Refresh();
        }
    }
}
