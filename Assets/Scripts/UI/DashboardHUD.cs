using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GG.Game; // <- use the Season models in GG.Game

namespace GG.Game
{
    public class DashboardHUD : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text weekText;
        [SerializeField] TMP_Text nextOpponentText;
        [SerializeField] Button simButton;
        [SerializeField] Button advanceButton;

        private SeasonState _season;
        private string Selected => GameState.SelectedTeamAbbr ?? PlayerPrefs.GetString("selected_team", "ATL");

        void Start()
        {
            var allAbbrs = GetAllTeamAbbrs();
            if (allAbbrs == null || allAbbrs.Count == 0)
            {
                Debug.LogError("[DashboardHUD] Could not enumerate teams from LeagueRepository.");
                allAbbrs = new List<string> { Selected };
            }

            // NEW signature: (selected, allAbbrs, generator)
            _season = SeasonState.LoadOrCreate(Selected, allAbbrs, ScheduleService.Generate);
            Refresh();
        }

        public void OnClickSim()
        {
            if (_season == null) return;

            var maybe = _season.GetNextGame(Selected);
            if (!maybe.HasValue) return;

            var g = maybe.Value;
            var engine = new LocalSimpleSim(abbr => RosterService.GetTeamOverall(abbr));
            var result = engine.Simulate(g, MakeSeed(g));
            _season.ApplyResult(result);
            _season.Save();
            Refresh();
        }

        public void OnClickAdvance()
        {
            if (_season == null) return;
            _season.week = Mathf.Clamp(_season.week + 1, 1, Math.Max(1, _season.schedule.Count));
            _season.Save();
            Refresh();
        }

        void Refresh()
        {
            if (_season == null) return;

            weekText?.SetText($"Week {_season.week}");

            var maybe = _season.GetNextGame(Selected);
            if (maybe.HasValue)
            {
                var g = maybe.Value;
                bool home = g.home.Equals(Selected, StringComparison.OrdinalIgnoreCase);
                string opp = home ? g.away : g.home;
                nextOpponentText?.SetText($"Next: {opp} {(home ? "(Home)" : "(Away)")}");
                simButton.interactable = true;
            }
            else
            {
                nextOpponentText?.SetText("No remaining games");
                simButton.interactable = false;
            }

            advanceButton.interactable = _season.week < Math.Max(1, _season.schedule.Count);
        }

        static int MakeSeed(GameInfo g)
        {
            unchecked
            {
                int w = g.week * 73856093;
                int h = (g.home?.GetHashCode() ?? 0) * 19349663;
                int a = (g.away?.GetHashCode() ?? 0) * 83492791;
                return w ^ h ^ a;
            }
        }

        // Robustly enumerate team abbreviations from LeagueRepository (no hard API dependency)
        static List<string> GetAllTeamAbbrs()
        {
            var abbrs = new List<string>();

            var repoType = Type.GetType("LeagueRepository") ??
                           AppDomain.CurrentDomain.GetAssemblies()
                                    .SelectMany(a => a.GetTypes())
                                    .FirstOrDefault(t => t.Name == "LeagueRepository");
            if (repoType == null) return abbrs;

            object teamsEnumerable = null;
            var prop = repoType.GetProperty("Teams", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (prop != null) teamsEnumerable = prop.GetValue(null, null);
            if (teamsEnumerable == null)
            {
                prop = repoType.GetProperty("AllTeams", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (prop != null) teamsEnumerable = prop.GetValue(null, null);
            }
            var field = repoType.GetField("Teams", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (teamsEnumerable == null && field != null) teamsEnumerable = field.GetValue(null);

            if (teamsEnumerable is IEnumerable seq)
            {
                foreach (var t in seq)
                {
                    var tt = t.GetType();
                    var abbrProp = tt.GetProperty("abbr") ?? tt.GetProperty("Abbr");
                    var abbrField = tt.GetField("abbr") ?? tt.GetField("Abbr");
                    var val = (string)(abbrProp?.GetValue(t) ?? abbrField?.GetValue(t));
                    if (!string.IsNullOrWhiteSpace(val))
                        abbrs.Add(val);
                }
            }

            if (abbrs.Count == 0)
            {
                var m = repoType.GetMethod("GetAllAbbrs", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (m != null)
                {
                    var ret = m.Invoke(null, null) as IEnumerable;
                    if (ret != null)
                        foreach (var s in ret) abbrs.Add(s?.ToString());
                }
            }

            return abbrs.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
