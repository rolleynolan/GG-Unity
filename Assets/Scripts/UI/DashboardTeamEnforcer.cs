using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GG.Game
{
    /// <summary>
    /// Forces the Dashboard header + logo (and roster panel) to the selected team,
    /// even if other scripts try to write defaults like "Washington".
    /// </summary>
    [DefaultExecutionOrder(10000)] // run very late
    public class DashboardTeamEnforcer : MonoBehaviour
    {
        const int ReapplyFrames = 6;  // re-assert for a few frames

        void Start()
        {
            if (SceneManager.GetActiveScene().name != "Dashboard") return;
            var abbr = ResolveAbbr();
            StartCoroutine(EnforceForAFewFrames(abbr));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "Dashboard") return;

            // Ensure an enforcer exists (no scene wiring needed)
            var existing = Object.FindFirstObjectByType<DashboardTeamEnforcer>(FindObjectsInactive.Include);
            if (!existing)
            {
                var go = new GameObject("DashboardTeamEnforcer");
                Object.DontDestroyOnLoad(go); // survives subloads in Dashboard
                go.AddComponent<DashboardTeamEnforcer>();
            }
        }

        static string ResolveAbbr()
        {
            var a = GameState.SelectedTeamAbbr;
            if (!string.IsNullOrEmpty(a)) return a;
            a = PlayerPrefs.GetString("selected_team", string.Empty);
            if (!string.IsNullOrEmpty(a)) return a;
            return "ATL"; // last resort
        }

        IEnumerator EnforceForAFewFrames(string abbr)
        {
            // Re-apply over a handful of frames to outlast late initializers/coroutines
            for (int i = 0; i < ReapplyFrames; i++)
            {
                ApplyHeaderAndLogo(abbr, out var title1, out var title2, out var logoName);
                ApplyRoster(abbr);
                if (i == 0)
                    Debug.Log($"[DashboardTeamEnforcer] Applying '{abbr}' (titles='{title1}|{title2}', logo='{logoName}'), frames={ReapplyFrames}");
                yield return new WaitForEndOfFrame();
            }
        }

        void ApplyRoster(string abbr)
        {
            var panel = FindFirstObjectByType<RosterPanelUI>(FindObjectsInactive.Include);
            panel?.ShowRosterForTeam(abbr);
        }

        // Finds two header texts if available (top and subline), otherwise uses/creates a single overlay text.
        void ApplyHeaderAndLogo(string abbr, out string title1, out string title2, out string logoName)
        {
            title1 = title2 = logoName = "-";

            // Load teams for city/name
            TeamData team = null;
            var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path).TrimStart();
                if (json.StartsWith("[")) json = "{\"teams\":" + json + "}";
                var list = JsonUtility.FromJson<TeamDataList>(json)?.teams;
                team = list?.FirstOrDefault(t => t.abbreviation.Equals(abbr, System.StringComparison.OrdinalIgnoreCase));
            }
            var lineTop = team != null ? team.city : abbr;
            var lineBottom = team != null ? $"{team.name} ({abbr})" : abbr;

            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (!canvas) return;
            var scope = canvas.transform;

            // Candidate TMPs: prefer ones already in the header area (named/positioned)
            var tmps = scope.GetComponentsInChildren<TMP_Text>(true);

            TMP_Text t1 = tmps.FirstOrDefault(x =>
                        x.name.IndexOf("Team", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        x.name.IndexOf("Title", System.StringComparison.OrdinalIgnoreCase) >= 0);
            TMP_Text t2 = tmps.Where(x => x != t1).FirstOrDefault(x =>
                        x.name.IndexOf("Sub", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        x.name.IndexOf("Name", System.StringComparison.OrdinalIgnoreCase) >= 0);

            // Fallback: create overlay texts pinned to the existing header bar (topmost Image across screen)
            if (!t1)
            {
                var headerBar = scope.GetComponentsInChildren<Image>(true)
                    .OrderByDescending(img => ((RectTransform)img.transform).rect.width) // widest
                    .FirstOrDefault();
                var parent = headerBar ? headerBar.transform : scope;

                var go1 = EnsureOverlayText(parent, "Header_Overlay_Top", 20, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -12));
                t1 = go1.GetComponent<TMP_Text>();
            }
            if (!t2)
            {
                var parent = t1.transform.parent;
                var go2 = EnsureOverlayText(parent, "Header_Overlay_Bot", 18, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -34));
                t2 = go2.GetComponent<TMP_Text>();
            }

            t1.text = lineTop;
            t2.text = lineBottom;
            title1 = t1.name;
            title2 = t2.name;

            // Logo: pick square-ish Image near those texts; else create one
            var images = scope.GetComponentsInChildren<Image>(true);
            Image logo = images
                .OrderBy(img => Vector3.Distance(img.transform.position, t1.transform.position))
                .ThenBy(img => Mathf.Abs(((RectTransform)img.transform).rect.width - ((RectTransform)img.transform).rect.height))
                .FirstOrDefault();

            if (!logo || ((RectTransform)logo.transform).rect.width < 10f)
            {
                var go = new GameObject("Header_Overlay_Logo", typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(t1.transform.parent, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(12, -8);
                rt.sizeDelta = new Vector2(40, 40);
                logo = go.GetComponent<Image>();
            }

            var spr = LogoService.Get(abbr);
            if (spr)
            {
                logo.sprite = spr;
                logo.preserveAspect = true;
                logo.enabled = true;
                logoName = logo.name;
            }
        }

        GameObject EnsureOverlayText(Transform parent, string name, float fontSize, Vector2 aMin, Vector2 aMax, Vector2 anchored)
        {
            var exist = parent.Find(name);
            if (exist) return exist.gameObject;

            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = aMax;
            rt.anchoredPosition = anchored;
            rt.sizeDelta = new Vector2(600, 32);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.enableAutoSizing = false;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = new Color(0.90f, 0.95f, 1f, 0.95f);
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return go;
        }
    }
}
