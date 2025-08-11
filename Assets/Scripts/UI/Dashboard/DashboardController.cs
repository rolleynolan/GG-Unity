using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
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

        private string abbr;
        private string city;
        private string name;

        private void Awake()
        {
            if (tabsController == null)
                tabsController = GetComponentInChildren<TabsController>(true);
        }

        private void Start()
        {
            var gs = GridironGM.GameState.Instance;
            if (gs == null || string.IsNullOrEmpty(gs.SelectedTeamAbbr))
            {
                Debug.LogError("[Dashboard] Missing team selection. Returning to TeamSelection.");
                SceneManager.LoadScene("TeamSelection");
                return;
            }

            abbr = gs.SelectedTeamAbbr.Trim().ToUpperInvariant();
            city = gs.SelectedTeamCity ?? string.Empty;
            name = gs.SelectedTeamName ?? string.Empty;

            if (teamTitle != null)
            {
                var title = string.IsNullOrEmpty(city + name) ? abbr : $"{city} {name} ({abbr})";
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

            if (rosterPanel != null)      rosterPanel.ShowRosterForTeam(abbr);
            if (depthChartPanel != null)  depthChartPanel.SetTeam(abbr);
            if (schedulePanel != null)    schedulePanel.SetTeam(abbr);

            EnsureTabs();
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

