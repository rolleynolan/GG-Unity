using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GG.Game;

namespace GG.Game
{
    /// <summary>
    /// Dashboard top bar controller. Deterministic bootstrap:
    ///  - Reads team ABBRs from StreamingAssets/teams.json
    ///  - Creates/loads SeasonState
    ///  - Generates a full schedule
    ///  - Wires Sim / Advance actions
    /// </summary>
    public class DashboardHUD : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] TMP_Text weekText;
        [SerializeField] TMP_Text nextOpponentText;
        [SerializeField] Button simButton;
        [SerializeField] Button advanceButton;

        private SeasonState _season;

        private string Selected =>
            GameState.SelectedTeamAbbr ?? PlayerPrefs.GetString("selected_team", "ATL");

        // --- AUTO-WIRE BUTTONS (always try) -----------------------------------------

        void Awake()
        {
            AutoWireUi();
        }

#if UNITY_EDITOR
        // Helpful in Editor after domain reloads / recompiles
        void OnValidate()
        {
            if (!Application.isPlaying) AutoWireUi();
        }
#endif

        void OnEnable()
        {
            // Re-add listeners in case domain reload removed them
            RegisterButtonListeners();
        }

        void OnDisable()
        {
            // Clean up to avoid duplicate invokes
            if (simButton != null) simButton.onClick.RemoveListener(OnClickSim);
            if (advanceButton != null) advanceButton.onClick.RemoveListener(OnClickAdvance);
        }

        /// <summary>
        /// Attempts to find missing UI references (buttons and optional texts) and wire them up.
        /// Prefer existing serialized refs, then try names in children, then whole-scene search.
        /// </summary>
        private void AutoWireUi()
        {
            // 1) Try to find buttons if not assigned
            if (simButton == null)
                simButton = FindButtonByCommonNames(new[] { "SimButton", "Sim", "Sim Game", "ButtonSim" });

            if (advanceButton == null)
                advanceButton = FindButtonByCommonNames(new[] { "AdvanceButton", "Advance", "NextWeek", "ButtonAdvance" });

            // (Optional) if you want the texts auto-wired too, uncomment:
            // if (weekText == null) weekText = FindTextByCommonNames(new[] { "WeekText", "Week", "TxtWeek" });
            // if (nextOpponentText == null) nextOpponentText = FindTextByCommonNames(new[] { "NextOpponentText", "Next", "TxtNextOpponent" });

            RegisterButtonListeners();
        }

        private void RegisterButtonListeners()
        {
            if (simButton != null)
            {
                simButton.onClick.RemoveListener(OnClickSim);
                simButton.onClick.AddListener(OnClickSim);
            }
            else
            {
                Debug.LogWarning("[DashboardHUD] Sim button not found. Assign it in the Inspector or name it 'SimButton'.");
            }

            if (advanceButton != null)
            {
                advanceButton.onClick.RemoveListener(OnClickAdvance);
                advanceButton.onClick.AddListener(OnClickAdvance);
            }
            else
            {
                Debug.LogWarning("[DashboardHUD] Advance button not found. Assign it in the Inspector or name it 'AdvanceButton'.");
            }
        }

        // --- find helpers ------------------------------------------------------------

        private Button FindButtonByCommonNames(IEnumerable<string> names)
        {
            // search under this HUD first (active or inactive)
            var inChildren = GetComponentsInChildren<Button>(true);
            foreach (var n in names)
            {
                var b = inChildren.FirstOrDefault(x => string.Equals(x.name, n, StringComparison.OrdinalIgnoreCase));
                if (b != null) return b;
            }

            // fall back to scene-wide search (includes inactive)
#if UNITY_2020_1_OR_NEWER
            var all = UnityEngine.Object.FindObjectsOfType<Button>(true);
#else
            var all = UnityEngine.Object.FindObjectsOfType<Button>();
#endif
            foreach (var n in names)
            {
                var b = all.FirstOrDefault(x => string.Equals(x.name, n, StringComparison.OrdinalIgnoreCase));
                if (b != null) return b;
            }

            return null;
        }

        private TMP_Text FindTextByCommonNames(IEnumerable<string> names)
        {
            var inChildren = GetComponentsInChildren<TMP_Text>(true);
            foreach (var n in names)
            {
                var t = inChildren.FirstOrDefault(x => string.Equals(x.name, n, StringComparison.OrdinalIgnoreCase));
                if (t != null) return t;
            }

#if UNITY_2020_1_OR_NEWER
            var all = UnityEngine.Object.FindObjectsOfType<TMP_Text>(true);
#else
            var all = UnityEngine.Object.FindObjectsOfType<TMP_Text>();
#endif
            foreach (var n in names)
            {
                var t = all.FirstOrDefault(x => string.Equals(x.name, n, StringComparison.OrdinalIgnoreCase));
                if (t != null) return t;
            }

            return null;
        }

        void Start()
        {
            var allAbbrs = LoadAbbrsFromTeamsJson();

            if (allAbbrs.Count <= 1)
            {
                Debug.LogWarning("[DashboardHUD] Could not load 2+ teams from teams.json. Using selected only.");
                allAbbrs = new List<string> { Selected };
            }

            _season = SeasonState.LoadOrCreate(
                Selected,
                allAbbrs,
                teams => ScheduleService.Generate(teams)
            );

            Refresh();
        }

        public void OnClickSim()
        {
            if (_season == null) return;

            var maybe = _season.GetNextGame(Selected);
            if (!maybe.HasValue) return;

            var g = maybe.Value;

            // Local sim using roster-derived OVR (falls back to 72 if not available)
            var engine = new LocalSimpleSim(TeamOverallFromRoster);
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

        private void Refresh()
        {
            if (_season == null) return;

            weekText?.SetText($"Week {_season.week}");

            var maybe = _season.GetNextGame(Selected);
            if (maybe.HasValue)
            {
                var g = maybe.Value;
                bool home = string.Equals(g.home, Selected, StringComparison.OrdinalIgnoreCase);
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

        // ---------------- helpers ----------------

        private static readonly Regex AbbrRegex =
            new Regex(@"""abbr""\s*:\s*""([A-Za-z_-]+)""", RegexOptions.Compiled);

        private static List<string> LoadAbbrsFromTeamsJson()
        {
            try
            {
                var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[DashboardHUD] teams.json not found at {path}");
                    return new List<string>();
                }

                var txt = File.ReadAllText(path);
                var matches = AbbrRegex.Matches(txt);

                var list = new List<string>(matches.Count);
                foreach (Match m in matches) list.Add(m.Groups[1].Value);

                list = list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                if (list.Count == 0)
                    Debug.LogWarning("[DashboardHUD] No abbr entries found in teams.json.");

                return list;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DashboardHUD] Failed to parse teams.json: {ex.Message}");
                return new List<string>();
            }
        }

        private static int MakeSeed(GameInfo g)
        {
            unchecked
            {
                int w = g.week * 73856093;
                int h = (g.home?.GetHashCode() ?? 0) * 19349663;
                int a = (g.away?.GetHashCode() ?? 0) * 83492791;
                return w ^ h ^ a;
            }
        }

        /// <summary>
        /// Derives a team overall rating from roster (if RosterService is present),
        /// otherwise returns a neutral default (72) to keep things moving.
        /// </summary>
        private static int TeamOverallFromRoster(string abbr)
        {
            try
            {
                var rsType = Type.GetType("RosterService") ??
                             AppDomain.CurrentDomain.GetAssemblies()
                               .SelectMany(a => a.GetTypes())
                               .FirstOrDefault(t => t.Name == "RosterService");

                if (rsType != null)
                {
                    var get = rsType.GetMethod("GetRosterForTeam",
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Static);

                    if (get != null)
                    {
                        var roster = get.Invoke(null, new object[] { abbr }) as System.Collections.IEnumerable;
                        if (roster != null)
                        {
                            int sum = 0, n = 0;
                            foreach (var p in roster)
                            {
                                int ovr = ExtractOvr(p);
                                if (ovr > 0) { sum += ovr; n++; }
                            }
                            if (n > 0)
                                return Mathf.Clamp(Mathf.RoundToInt(sum / (float)n), 55, 95);
                        }
                    }
                }
            }
            catch { /* ignore and use default */ }

            return 72;
        }

        private static int ExtractOvr(object player)
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
    }
}

