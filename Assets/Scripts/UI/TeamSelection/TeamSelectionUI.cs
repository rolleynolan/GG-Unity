using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TeamRow
{
    public Button button;      // root button
    public TMP_Text label;     // shows "CITY NAME (ABBR)"
}

public class TeamSelectionUI : MonoBehaviour
{
    [Header("Prefabs & Targets")]
    [SerializeField] private Transform contentParent;    // ScrollView/Viewport/Content
    [SerializeField] private GameObject teamRowPrefab;   // prefab with Button + TMP_Text
    [SerializeField] private TMP_Text headerText;        // optional: “Select a Team”
    [SerializeField] private RosterPanelUI rightPanel;   // optional: preview panel at right

    private Team[] teams;

    void Start()
    {
        PopulateTeams();
    }

    public void PopulateTeams()
    {
        teams = LeagueRepository.GetTeams();
        if (teams == null || teams.Length == 0)
        {
            headerText?.SetText("No teams found. Place teams.json in Resources/Config or persistentDataPath.");
            Debug.LogWarning("[TeamSelectionUI] No teams loaded.");
            return;
        }

        headerText?.SetText("Select a Team");

        // Clear existing
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        foreach (var t in teams.OrderBy(t => t.abbreviation))
        {
            var go = Instantiate(teamRowPrefab, contentParent);
            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<TMP_Text>();
            label.SetText($"{t.city} {t.name} ({t.abbreviation})");

            string abbr = t.abbreviation; // capture
            btn.onClick.AddListener(() =>
            {
                GameSession.SelectedTeamAbbr = abbr;
                Debug.Log($"[TeamSelectionUI] Selected {abbr}");
                rightPanel?.SetTeam(abbr); // preview roster if you wired it
            });
        }

        // Layout rebuild
        Canvas.ForceUpdateCanvases();
        var rt = contentParent as RectTransform;
        if (rt) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        Canvas.ForceUpdateCanvases();
    }
}
