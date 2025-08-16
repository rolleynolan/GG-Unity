using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GG.Game;

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
            // Defer init until LeagueRepository (or a fallback) is ready
            StartCoroutine(BootstrapWhenReady());
        }

        IEnumerator BootstrapWhenReady()
        {
            // Try for ~2 seconds to get a non-empty team list
            List<string> allAbbrs = null;
            float timeout = 2f;
            while (timeout > 0f)
            {
                allAbbrs = GetAllTeamAbbrs(silent: true);
                if (allAbbrs != null && allAbbrs.Count > 0) break;

                // fallback source: try roster keys if available
                allAbbrs = TryGetAbbrsFromRosterService();
                if (allAbbrs != null && allAbbrs.Count > 0) break;

                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            if (allAbbrs == null || allAbbrs.Count == 0)
            {
                Debug.LogWarning("[DashboardHUD] Team list still empty after waiting; using selected only.");
                allAbbrs = new List<string> { Selected };
            }

            _season = SeasonState.LoadOrCreate(Selected, allAbbrs, teams => ScheduleService.Generate(teams));
            Refresh();
        }

        public void OnClickSim()
        {
            if (_season == null) return;

            var maybe = _season.GetNextGame(Selected);
            if (!maybe.HasValue) return;

            var g = maybe.Value;
            var engine = new LocalSimpleSim(abbr => TeamOverallFromRoster(abbr));
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

        // ------------ helpers ------------

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

        // Compute team overall from roster (reflection-friendly)
        static int TeamOverallFromRoster(string abbr)
        {
            var roster = TryGetRosterEnumerable(abbr);
            if (roster == null) return 72;

            int sum = 0, n = 0;
            foreach (var p in roster)
            {
                int ovr = ExtractOvr(p);
                if (ovr > 0) { sum += ovr; n++; }
            }

            if (n == 0) return 72;
            var avg = Mathf.RoundToInt(sum / (float)n);
            return Mathf.Clamp(avg, 55, 95);
        }

        static IEnumerable TryGetRosterEnumerable(string abbr)
        {
            var rsType = Type.GetType("RosterService") ??
                         AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .FirstOrDefault(t => t.Name == "RosterService");
            if (rsType == null) return null;

            object roster = null;

            // Common static methods
            var methods = new[]
            {
                "GetRosterForTeam", "GetRoster", "GetPlayersForTeam",
                "GetByAbbr", "Get"
            };

            foreach (var name in methods)
            {
                var mi = rsType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (mi == null) continue;
                var pars = mi.GetParameters();
                if (pars.Length == 1 && pars[0].ParameterType == typeof(string))
                {
                    roster = mi.Invoke(null, new object[] { abbr });
                    break;
                }
            }

            // Some services expose: Dictionary<string, List<Player>> Rosters
            if (roster == null)
            {
                var field = rsType.GetField("Rosters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (field != null)
                {
                    var dict = field.GetValue(null);
                    if (dict != null)
                    {
                        var idx = dict.GetType().GetMethod("get_Item");
                        if (idx != null) roster = idx.Invoke(dict, new object[] { abbr });
                    }
                }
            }

            return roster as IEnumerable;
        }

        static int ExtractOvr(object player)
        {
            if (player == null) return 0;
            var t = player.GetType();

            var prop = t.GetProperty("ovr") ?? t.GetProperty("OVR") ??
                       t.GetProperty("overall") ?? t.GetProperty("Overall") ??
                       t.GetProperty("rating") ?? t.GetProperty("Rating");
            if (prop != null)
            {
                var val = prop.GetValue(player);
                if (val is int vi) return vi;
                if (val != null && int.TryParse(val.ToString(), out var pi)) return pi;
            }

            var field = t.GetField("ovr") ?? t.GetField("OVR") ??
                        t.GetField("overall") ?? t.GetField("Overall") ??
                        t.GetField("rating") ?? t.GetField("Rating");
            if (field != null)
            {
                var val = field.GetValue(player);
                if (val is int vi) return vi;
                if (val != null && int.TryParse(val.ToString(), out var pi)) return pi;
            }

            return 0;
        }

        // --- Team list discovery ---

        static List<string> GetAllTeamAbbrs(bool silent = false)
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

            if (abbrs.Count == 0 && !silent)
                Debug.Log("[DashboardHUD] LeagueRepository found but team list is empty (still loading?).");

            return abbrs.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        // Second fallback: pull keys from RosterService if it holds a dictionary of rosters
        static List<string> TryGetAbbrsFromRosterService()
        {
            var result = new List<string>();
            var rsType = Type.GetType("RosterService") ??
                         AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .FirstOrDefault(t => t.Name == "RosterService");
            if (rsType == null) return result;

            var field = rsType.GetField("Rosters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null) return result;

            var dict = field.GetValue(null);
            if (dict == null) return result;

            var keysProp = dict.GetType().GetProperty("Keys");
            var keys = keysProp?.GetValue(dict) as IEnumerable;
            if (keys != null)
                foreach (var k in keys) result.Add(k?.ToString());

            return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}

