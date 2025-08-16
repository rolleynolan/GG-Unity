using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GG.Bridge.Repositories;

namespace GG.UI.Dashboard
{
    public class DashboardSceneController : MonoBehaviour
    {
        [Header("Optional references (found by name if null)")]
        public Button SimGameButton;
        public Button SimWeekButton;
        public Button SimSeasonButton;
        public TMP_Text HeaderTeam;

        public string SelectedTeamAbbr = "ATL";   // fallback if selection system not wired

        void Awake()
        {
            // Try find buttons by common names if not assigned
            if (!SimGameButton)  SimGameButton  = GameObject.Find("SimGameButton")?.GetComponent<Button>();
            if (!SimWeekButton)  SimWeekButton  = GameObject.Find("SimWeekButton")?.GetComponent<Button>();
            if (!SimSeasonButton)SimSeasonButton= GameObject.Find("SimSeasonButton")?.GetComponent<Button>();
            if (!HeaderTeam)     HeaderTeam     = GameObject.Find("HeaderTeam")?.GetComponent<TMP_Text>();

            LeagueRepository.LoadTeams();
            if (HeaderTeam) HeaderTeam.text = $"{SelectedTeamAbbr}";

            // Load or build a tiny schedule so buttons can work immediately
            if (!ScheduleRepository.TryLoad(out _))
                ScheduleRepository.AutoGenerateIfMissing(LeagueRepository.TeamAbbrs(), 2026);

            WireButtons();
            RefreshInteractivity();
        }

        void WireButtons()
        {
            if (SimGameButton)  { SimGameButton.onClick.RemoveAllListeners();  SimGameButton.onClick.AddListener(OnSimGame); }
            if (SimWeekButton)  { SimWeekButton.onClick.RemoveAllListeners();  SimWeekButton.onClick.AddListener(OnSimWeek); }
            if (SimSeasonButton){ SimSeasonButton.onClick.RemoveAllListeners();SimSeasonButton.onClick.AddListener(OnSimSeason); }
        }

        void OnSimGame()
        {
            ScheduleRepository.SimNextGameForTeam(SelectedTeamAbbr);
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
            while (ScheduleRepository.HasUnplayedThisWeek(SelectedTeamAbbr, out _))
                ScheduleRepository.SimEntireWeek();
            RefreshInteractivity();
            BroadcastMessage("RefreshTeamSchedule", SendMessageOptions.DontRequireReceiver);
        }

        void RefreshInteractivity()
        {
            var hasGame = ScheduleRepository.HasUnplayedThisWeek(SelectedTeamAbbr, out _);
            if (SimGameButton)  SimGameButton.interactable  = hasGame;
            if (SimWeekButton)  SimWeekButton.interactable  = true; // allow sim week always once schedule exists
            if (SimSeasonButton)SimSeasonButton.interactable= true;
        }
    }
}
