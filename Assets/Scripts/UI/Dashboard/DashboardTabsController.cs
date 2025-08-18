using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-500)]
public class DashboardTabsController : MonoBehaviour
{
    [Header("Optional: assign explicitly; else found by name")]
    [SerializeField] private GameObject rosterPanel;
    [SerializeField] private GameObject depthChartsPanel;
    [SerializeField] private GameObject teamSchedulePanel;

    [SerializeField] private Transform tabsRoot;

    void Awake()
    {
        rosterPanel = rosterPanel ?? GameObject.Find("RosterPanel");
        depthChartsPanel = depthChartsPanel ?? GameObject.Find("DepthChartsPanel");
        teamSchedulePanel = teamSchedulePanel ?? GameObject.Find("TeamSchedulePanel");

        SetActiveSafe(rosterPanel, true);
        SetActiveSafe(depthChartsPanel, false);
        SetActiveSafe(teamSchedulePanel, false);

        var searchRoot = tabsRoot ? tabsRoot : transform;
        var buttons = searchRoot.GetComponentsInChildren<Button>(true).ToList();
        if (buttons.Count == 0)
        {
            buttons = UnityEngine.Object
                .FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .ToList();
        }

        foreach (var btn in buttons)
        {
            string label = GetButtonLabel(btn).ToLowerInvariant().Replace(" ", "");
            if (label.Contains("roster"))
            {
                btn.onClick.AddListener(() => Show(roster: true));
            }
            else if (label.Contains("depth"))
            {
                btn.onClick.AddListener(() => Show(depth: true));
            }
            else if (label.Contains("schedule"))
            {
                btn.onClick.AddListener(() => Show(schedule: true));
            }
        }
    }

    static string GetButtonLabel(Button b)
    {
        var tmp = b.GetComponentInChildren<TMP_Text>(true);
        if (tmp) return tmp.text;
        var uText = b.GetComponentInChildren<UnityEngine.UI.Text>(true);
        if (uText) return uText.text;
        return b.gameObject.name;
    }

    void Show(bool roster = false, bool depth = false, bool schedule = false)
    {
        SetActiveSafe(rosterPanel, roster);
        SetActiveSafe(depthChartsPanel, depth);
        SetActiveSafe(teamSchedulePanel, schedule);
    }

    static void SetActiveSafe(GameObject go, bool on)
    {
        if (go && go.activeSelf != on)
        {
            go.SetActive(on);
        }
    }
}
