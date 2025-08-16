using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
            StartCoroutine(BootstrapWhenReady());
        }

        IEnumerator BootstrapWhenReady()
        {
            // Try to proactively kick the repo (if it exposes Load/Init).
            TryInvokeRepoLoad();

            // Wait up to 6s for a team list (prefer >1).
            List<string> allAbbrs = null;
            float timeout = 6f;
            while (timeout > 0f)
            {
                allAbbrs = GetAllTeamAbbrs(silent: true);
                if (allAbbrs != null && allAbbrs.Count > 1) break;

                var viaRoster = TryGetAbbrsFromRosterService();
                if (viaRoster != null && viaRoster.Count > 1) { allAbbrs = viaRoster; break; }

                var viaJson = TryGetAbbrsFromTeamsJson();
                if (viaJson != null && viaJson.Count > 1) { allAbbrs = viaJson; break; }

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
            var rsType = FindTypeByName("RosterService");
            if (rsType == null) return null;

            object roster = null;

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

        static void TryInvokeRepoLoad()
        {
            var repoType = FindTypeByName("LeagueRepository");
            if (repoType == null) return;

            // static Load/Init
            foreach (var name in new[] { "Load", "Init", "Initialize" })
            {
                var m = repoType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (m != null) { try { m.Invoke(null, null); } catch {} }
            }

            // instance Load/Init on Instance/Current
            var inst = GetSingleton(repoType);
            if (inst != null)
            {
                foreach (var name in new[] { "Load", "Init", "Initialize" })
                {
                    var mi = repoType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (mi != null) { try { mi.Invoke(inst, null); } catch {} }
                }
            }
        }

        static List<string> GetAllTeamAbbrs(bool silent = false)
        {
            var abbrs = new List<string>();
            var repoType = FindTypeByName("LeagueRepository");
            if (repoType == null) return abbrs;

            // Scan static then instance members and recursively extract abbrs
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            TryExtractFromRepo(repoType, /*instance*/ null, abbrs, visited);
            var inst = GetSingleton(repoType);
            if (inst != null) TryExtractFromRepo(repoType, inst, abbrs, visited);

            if (abbrs.Count == 0 && !silent)
                Debug.Log("[DashboardHUD] LeagueRepository found but team list is empty (still loading?).");

            return abbrs.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        static void TryExtractFromRepo(Type repoType, object repoObj, List<string> sink, HashSet<object> visited)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic |
                        (repoObj == null ? BindingFlags.Static : BindingFlags.Instance);

            // Properties
            foreach (var p in repoType.GetProperties(flags))
            {
                if (!p.CanRead) continue;
                object val = null;
                try { val = p.GetValue(repoObj, null); } catch { }
                ExtractAbbrsFromAny(val, sink, visited, 3);
            }
            // Fields
            foreach (var f in repoType.GetFields(flags))
            {
                object val = null;
                try { val = f.GetValue(repoObj); } catch { }
                ExtractAbbrsFromAny(val, sink, visited, 3);
            }
            // Parameterless methods that look like getters
            foreach (var m in repoType.GetMethods(flags))
            {
                if (m.GetParameters().Length != 0) continue;
                if (m.ReturnType == typeof(void)) continue;
                if (!LooksLikeGetterName(m.Name)) continue;
                object val = null;
                try { val = m.Invoke(repoObj, null); } catch { }
                ExtractAbbrsFromAny(val, sink, visited, 2);
            }
        }

        static bool LooksLikeGetterName(string n)
        {
            return n.StartsWith("Get", StringComparison.OrdinalIgnoreCase) ||
                   n.StartsWith("All", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("Teams", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("Values", StringComparison.OrdinalIgnoreCase) ||
                   n.Equals("List", StringComparison.OrdinalIgnoreCase);
        }

        // Recursively walk dictionaries/enumerables/objects and pull "abbr" strings or plausible codes.
        static void ExtractAbbrsFromAny(object obj, List<string> sink, HashSet<object> visited, int depth)
        {
            if (obj == null || depth < 0) return;

            // Avoid cycles
            if (!obj.GetType().IsValueType)
            {
                if (visited.Contains(obj)) return;
                visited.Add(obj);
            }

            // String?
            if (obj is string s)
            {
                if (LooksLikeAbbr(s)) sink.Add(s);
                return;
            }

            // IDictionary: take keys + recurse values
            if (obj is IDictionary dict)
            {
                foreach (var key in dict.Keys)
                    if (key is string ks && LooksLikeAbbr(ks)) sink.Add(ks);
                foreach (var val in dict.Values)
                    ExtractAbbrsFromAny(val, sink, visited, depth - 1);
                return;
            }

            // IEnumerable: recurse
            if (obj is IEnumerable seq)
            {
                foreach (var it in seq) ExtractAbbrsFromAny(it, sink, visited, depth - 1);
                return;
            }

            // Object with "abbr"/"Abbr" property/field
            var t = obj.GetType();
            var ap = t.GetProperty("abbr") ?? t.GetProperty("Abbr") ?? t.GetProperty("id") ?? t.GetProperty("ID");
            if (ap != null)
            {
                try
                {
                    var v = ap.GetValue(obj)?.ToString();
                    if (!string.IsNullOrWhiteSpace(v) && LooksLikeAbbr(v)) sink.Add(v);
                }
                catch { }
            }
            var af = t.GetField("abbr") ?? t.GetField("Abbr") ?? t.GetField("id") ?? t.GetField("ID");
            if (af != null)
            {
                try
                {
                    var v = af.GetValue(obj)?.ToString();
                    if (!string.IsNullOrWhiteSpace(v) && LooksLikeAbbr(v)) sink.Add(v);
                }
                catch { }
            }
        }

        static bool LooksLikeAbbr(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();
            // Typical: 2–4 letters (case-insensitive), allow one hyphen (e.g., "LA", "TB", "WAS", "BUF", "N-ATL")
            if (s.Length < 2 || s.Length > 6) return false;
            int letters = 0;
            foreach (var c in s)
            {
                if (char.IsLetter(c)) letters++;
                else if (c == '-' || c == '_') continue;
                else return false;
            }
            return letters >= 2;
        }

        static List<string> TryGetAbbrsFromRosterService()
        {
            var result = new List<string>();
            var rsType = FindTypeByName("RosterService");
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

        static List<string> TryGetAbbrsFromTeamsJson()
        {
            try
            {
                var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
                if (!File.Exists(path)) return new List<string>();
                var txt = File.ReadAllText(path);
                var m = Regex.Matches(txt, "\"abbr\"\\s*:\\s*\"([A-Za-z\\-_]+)\");
                var list = new List<string>(m.Count);
                foreach (Match mm in m) list.Add(mm.Groups[1].Value);
                return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }
            catch { return new List<string>(); }
        }

        static Type FindTypeByName(string typeName)
        {
            var t = Type.GetType(typeName);
            if (t != null) return t;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var found = asm.GetTypes().FirstOrDefault(x => x.Name == typeName);
                    if (found != null) return found;
                }
                catch { }
            }
            return null;
        }

        static object GetSingleton(Type type)
        {
            var p = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                 ?? type.GetProperty("Current",  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                 ?? type.GetProperty("Singleton",BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (p != null) { try { return p.GetValue(null, null); } catch { } }

            var f = type.GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                 ?? type.GetField("Current",  BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                 ?? type.GetField("Singleton",BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (f != null) { try { return f.GetValue(null); } catch { } }

            return null;
        }

        // ReferenceEqualityComparer for the visited set
        sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
