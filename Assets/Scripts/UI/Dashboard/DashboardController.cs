using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

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

        private void Awake()
        {
            if (tabsController == null)
                tabsController = GetComponentInChildren<TabsController>(true);
        }

        private void Start()
        {
            string abbr = GameSession.SelectedTeamAbbr;
            var team = LeagueRepository.GetTeams().FirstOrDefault(t => t.abbreviation == abbr);
            string city = team?.city ?? string.Empty;
            string name = team?.name ?? string.Empty;

            if (teamTitle != null)
            {
                var title = string.IsNullOrEmpty(city + name) ? abbr : $"{city} {name} ({abbr})";
                teamTitle.text = title;
            }

            if (subline != null)
            {
                subline.text = $"Week {GameSession.CurrentWeek}";
            }

            if (teamLogo != null)
            {
                var sprite = Resources.Load<Sprite>($"TeamSprites/{abbr}");
                teamLogo.sprite = sprite;
                teamLogo.enabled = sprite != null;
                teamLogo.preserveAspect = true;
            }

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
