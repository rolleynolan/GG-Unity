using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace GG.UI.Dashboard
{
    public class DashboardSceneController : MonoBehaviour
    {
        [Header("Optional references (auto-wired in editor if null)")]
        [SerializeField, FormerlySerializedAs("SimGameButton")] Button simGameButton;
        [SerializeField, FormerlySerializedAs("SimWeekButton")] Button simWeekButton;
        [SerializeField, FormerlySerializedAs("SimSeasonButton")] Button simSeasonButton;
        [SerializeField, FormerlySerializedAs("HeaderTeam")] TMP_Text headerTeam;
        [SerializeField, FormerlySerializedAs("SelectedTeamAbbr")] string selectedTeamAbbr = "";
        [SerializeField] TMP_Text statusHud;

        static List<string> LoadTeamAbbrs()
        {
            return new List<string> { "ATL", "PHI", "DAL", "NYG" };
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!simGameButton)   simGameButton   = GameObject.Find("SimGameButton")?.GetComponent<Button>();
            if (!simWeekButton)   simWeekButton   = GameObject.Find("SimWeekButton")?.GetComponent<Button>();
            if (!simSeasonButton) simSeasonButton = GameObject.Find("SimSeasonButton")?.GetComponent<Button>();
            if (!headerTeam)      headerTeam      = GameObject.Find("HeaderTeam")?.GetComponent<TMP_Text>();
        }
#endif

        void Awake()
        {
            var abbrs = LoadTeamAbbrs();
            if (string.IsNullOrEmpty(selectedTeamAbbr)) selectedTeamAbbr = abbrs.Count > 0 ? abbrs[0] : "ATL";
            if (headerTeam) headerTeam.text = selectedTeamAbbr;

            if (!ScheduleRepository.TryLoad(out _))
                ScheduleRepository.AutoGenerateIfMissing(abbrs, 2026);

            CreateStatusHud();
            WireButtons();
            BroadcastMessage("RefreshTeamSchedule", SendMessageOptions.DontRequireReceiver);
            RefreshInteractivity();
        }

        void WireButtons()
        {
            if (simGameButton)   { simGameButton.onClick.RemoveAllListeners();   simGameButton.onClick.AddListener(OnSimGame); }
            if (simWeekButton)   { simWeekButton.onClick.RemoveAllListeners();   simWeekButton.onClick.AddListener(OnSimWeek); }
            if (simSeasonButton) { simSeasonButton.onClick.RemoveAllListeners(); simSeasonButton.onClick.AddListener(OnSimSeason); }
        }

        void OnSimGame()
        {
            ScheduleRepository.SimNextGameForTeam(selectedTeamAbbr);
            BroadcastMessage("RefreshTeamSchedule", SendMessageOptions.DontRequireReceiver);
            RefreshInteractivity();
        }

        void OnSimWeek()
        {
            ScheduleRepository.SimEntireWeek();
            BroadcastMessage("RefreshTeamSchedule", SendMessageOptions.DontRequireReceiver);
            RefreshInteractivity();
        }

        void OnSimSeason()
        {
            int guard = 256;
            while (guard-- > 0 && ScheduleRepository.HasUnplayedThisWeek(selectedTeamAbbr, out _))
                ScheduleRepository.SimEntireWeek();

            BroadcastMessage("RefreshTeamSchedule", SendMessageOptions.DontRequireReceiver);
            RefreshInteractivity();
        }

        void RefreshInteractivity()
        {
            bool hasGame = ScheduleRepository.HasUnplayedThisWeek(selectedTeamAbbr, out var next);
            if (simGameButton)   simGameButton.interactable   = hasGame;
            if (simWeekButton)   simWeekButton.interactable   = true;
            if (simSeasonButton) simSeasonButton.interactable = true;

            var weekIdx = ScheduleRepository.CurrentWeekIndex() + 1;
            if (statusHud)
            {
                var vs = hasGame ? (next.home == selectedTeamAbbr ? $"vs {next.away}" : $"@ {next.home}") : "—";
                statusHud.text = $"Week {weekIdx} • Next: {vs}";
            }
        }

        void CreateStatusHud()
        {
            var existing = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                           .FirstOrDefault(t => t.name == "GG_StatusHUD");
            if (existing)
            {
                statusHud = existing;
                return;
            }

            var go = new GameObject("GG_StatusHUD", typeof(RectTransform));
            go.transform.SetParent(FindCanvasTransform(), false);

            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 18;
            txt.alignment = TextAlignmentOptions.TopLeft;

            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(10, -10);

            statusHud = txt;
        }

        Transform FindCanvasTransform()
        {
            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            return canvas ? canvas.transform : transform;
        }
    }
}
