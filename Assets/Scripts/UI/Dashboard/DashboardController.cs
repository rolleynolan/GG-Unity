using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using GridironGM.Data;
using GridironGM.UI.TeamSelection;

namespace GridironGM.UI.Dashboard
{
    public class DashboardController : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text teamTitle;     // "City Name (ABBR)"
        [SerializeField] private TMP_Text subline;       // "Week —  •  Year —" (stub for now)
        [SerializeField] private Image teamLogo;         // Image named "Logo" in header

        [Header("Tabs & Panels")]
        [SerializeField] private TabsController tabsController; // optional; auto-wires if found
        [SerializeField] private Toggle tabRoster;
        [SerializeField] private Toggle tabDepth;
        [SerializeField] private Toggle tabSchedule;

        [SerializeField] private GameObject panelRoster;
        [SerializeField] private GameObject panelDepth;
        [SerializeField] private GameObject panelSchedule;

        [Header("Content Controllers")]
        [SerializeField] private RosterPanelUI rosterPanel;         // existing component
        [SerializeField] private DepthChartPanel depthChartPanel;   // stub
        [SerializeField] private SchedulePanel schedulePanel;       // stub

        [Header("Roster Filters")]
        [SerializeField] private TMP_Dropdown positionFilter, sortFilter;
        [SerializeField] private TMP_InputField searchInput;
        private List<PlayerDTO> teamRoster = new();

        private string abbr;
        private string city;
        private string teamName;

        private void Awake()
        {
            if (tabsController == null)
                tabsController = GetComponentInChildren<TabsController>(true);
        }

        private void Start()
        {
            var gs = GridironGM.GameState.Instance;
            // Auto-find rosterPanel if unassigned
            if (rosterPanel == null)
                rosterPanel = GetComponentInChildren<RosterPanelUI>(true);
            if (gs == null || string.IsNullOrEmpty(gs.SelectedTeamAbbr))
            {
                Debug.LogError("[Dashboard] Missing team selection. Returning to TeamSelection.");
                SceneManager.LoadScene("TeamSelection");
                return;
            }

            abbr = gs.SelectedTeamAbbr.Trim().ToUpperInvariant();
            city = gs.SelectedTeamCity ?? string.Empty;
            teamName = gs.SelectedTeamName ?? string.Empty;

            if (teamTitle != null)
            {
                var title = string.IsNullOrEmpty(city + teamName) ? abbr : $"{city} {teamName} ({abbr})";
                teamTitle.text = title;
            }

            if (subline != null)
            {
                // TODO: wire actual week/year/record later
                subline.text = "Week —  •  Year —";
            }

            if (teamLogo != null)
            {
                var sprite = Resources.Load<Sprite>($"TeamSprites/{abbr}");
                teamLogo.sprite = sprite;
                teamLogo.enabled = sprite != null;
                teamLogo.preserveAspect = true;
            }

            if (rosterPanel != null)
            {
                rosterPanel.ShowRosterForTeam(abbr);
                teamRoster = RosterPanelUI.FetchRosterList(abbr);
                ApplyFiltersAndRebuild();
                WireFilterEvents();
            }
            if (depthChartPanel != null)  depthChartPanel.SetTeam(abbr);
            if (schedulePanel != null)    schedulePanel.SetTeam(abbr);

            EnsureTabs();
        }

        private void WireFilterEvents()
        {
            if (positionFilter) positionFilter.onValueChanged.AddListener(_ => ApplyFiltersAndRebuild());
            if (sortFilter)     sortFilter.onValueChanged.AddListener(_ => ApplyFiltersAndRebuild());
            if (searchInput)    searchInput.onValueChanged.AddListener(_ => ApplyFiltersAndRebuild());
        }

        private void ApplyFiltersAndRebuild()
        {
            if (teamRoster == null || rosterPanel == null) return;

            string pos = positionFilter ? positionFilter.options[positionFilter.value].text : "All";
            string q   = searchInput ? (searchInput.text ?? string.Empty).Trim().ToLowerInvariant() : string.Empty;
            string sort= sortFilter ? sortFilter.options[sortFilter.value].text : "OVR";

            var filtered = teamRoster;
            if (!string.IsNullOrEmpty(q))
                filtered = filtered.FindAll(p => p.name.ToLowerInvariant().Contains(q));

            if (pos != "All")
                filtered = filtered.FindAll(p => p.position == pos);

            switch (sort)
            {
                case "Name": filtered.Sort((a,b)=> string.Compare(a.name,b.name,System.StringComparison.Ordinal)); break;
                case "Age":  filtered.Sort((a,b)=> a.age.CompareTo(b.age)); break;
                default:      filtered.Sort((a,b)=> b.ovr.CompareTo(a.ovr)); break; // OVR desc
            }

            rosterPanel.RebuildFromList(filtered);
        }

        private void EnsureTabs()
        {
            if (tabsController != null &&
                tabRoster != null && tabDepth != null && tabSchedule != null &&
                panelRoster != null && panelDepth != null && panelSchedule != null)
            {
                tabsController.Configure(
                    new Toggle[] { tabRoster, tabDepth, tabSchedule },
                    new GameObject[] { panelRoster, panelDepth, panelSchedule },
                    defaultActiveIndex: 0
                );
            }
            else
            {
                if (panelRoster != null) panelRoster.SetActive(true);
                if (panelDepth != null) panelDepth.SetActive(false);
                if (panelSchedule != null) panelSchedule.SetActive(false);
                if (tabRoster != null) tabRoster.isOn = true;
            }
        }
    }
}

