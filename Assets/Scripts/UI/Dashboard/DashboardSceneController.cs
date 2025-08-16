using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using GG.Bridge.Repositories;
using System.IO;


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

        [System.Serializable] private class _Team { public string city; public string name; public string abbreviation; }
            [System.Serializable] private class _TeamRoot { public _Team[] teams; }

            private static System.Collections.Generic.List<string> LoadTeamAbbrs()
            {
                var list = new System.Collections.Generic.List<string>();
                try
                {
                    var path = Path.Combine(UnityEngine.Application.streamingAssetsPath, "teams.json");
                    if (File.Exists(path))
                    {
                        var json = File.ReadAllText(path);
                        var root = JsonUtility.FromJson<_TeamRoot>(json);
                        if (root?.teams != null)
                            foreach (var t in root.teams)
                                if (!string.IsNullOrEmpty(t.abbreviation))
                                    list.Add(t.abbreviation);
                    }
                }
                catch { /* fall through */ }

                if (list.Count == 0) list.AddRange(new[] { "ATL", "PHI", "DAL", "NYG" });
                return list;
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
            // option A: fully-qualified (works even without using)
            // old (causing error)
// var abbrs = GG.Bridge.Repositories.TeamDirectory.GetAbbrs();

// new
            var abbrs = LoadTeamAbbrs();


            if (string.IsNullOrEmpty(selectedTeamAbbr)) selectedTeamAbbr = abbrs.Count > 0 ? abbrs[0] : "ATL";
            if (headerTeam) headerTeam.text = selectedTeamAbbr;

            if (!ScheduleRepository.TryLoad(out _))
                ScheduleRepository.AutoGenerateIfMissing(abbrs, 2026);

            WireButtons();
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
            RefreshInteractivity();
            BroadcastMessage("RefreshTeamSchedule", SendMessageOptions.DontRequireReceiver);
        }

        void OnSimWeek()
        {
            ScheduleRepository.SimEntireWeek();
            RefreshInteractivity();
            BroadcastMessage("RefreshTeamSchedule", SendMessageOptions.DontRequireReceiver);
        }

        void OnSimSeason()
        {
            while (ScheduleRepository.HasUnplayedThisWeek(selectedTeamAbbr, out _))
                ScheduleRepository.SimEntireWeek();
            RefreshInteractivity();
            BroadcastMessage("RefreshTeamSchedule", SendMessageOptions.DontRequireReceiver);
        }

        void RefreshInteractivity()
        {
            var hasGame = ScheduleRepository.HasUnplayedThisWeek(selectedTeamAbbr, out _);
            if (simGameButton)   simGameButton.interactable   = hasGame;
            if (simWeekButton)   simWeekButton.interactable   = true;
            if (simSeasonButton) simSeasonButton.interactable = true;
        }
    }
}
