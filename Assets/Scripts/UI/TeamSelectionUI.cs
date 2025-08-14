using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;                      // for OrderBy
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionUI : MonoBehaviour
{
    [Header("Left List")]
    [SerializeField] private RectTransform listContent;   // Scroll/Viewport/Content
    [SerializeField] private GameObject teamRowPrefab;    // prefab with TeamRowUI or TeamRowBinder

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;

    [Header("Right Panel Preview")]
    [SerializeField] private RosterPanelUI preview;       // assign the right panel script

    private string selectedAbbr = "";

    void Start()
    {
        if (!listContent || !teamRowPrefab) AutoWireList();
        if (confirmButton) confirmButton.interactable = false;

        var teams = LoadTeamsFromStreamingAssets();       // <— no LeagueRepository dependency

        if (teams == null || teams.Count == 0)
        {
            Debug.LogError("[TeamSelectionUI] No teams loaded from StreamingAssets/teams.json");
            return;
        }

        // Clear existing children
        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);

        // Populate rows
        foreach (var t in teams.OrderBy(x => x.city).ThenBy(x => x.name))
        {
            var row = Instantiate(teamRowPrefab, listContent);

            // Prefer TeamRowUI if the prefab has it; otherwise fall back to TeamRowBinder + Button
            var ui = row.GetComponent<TeamRowUI>();
            if (ui != null)
            {
                ui.Set(t, () => OnTeamClicked(t.abbreviation));
            }
            else
            {
                var binder = row.GetComponent<TeamRowBinder>() ?? row.AddComponent<TeamRowBinder>();
                binder.Set(t);

                var btn = row.GetComponent<Button>() ?? row.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnTeamClicked(t.abbreviation));
            }
        }

        Debug.Log($"[TeamSelectionUI] Teams count: {teams.Count}");
    }

    void OnTeamClicked(string abbr)
    {
        selectedAbbr = abbr;
        if (confirmButton) confirmButton.interactable = true;
        preview?.ShowRosterForTeam(abbr);                 // Right panel shows roster immediately
        Debug.Log($"[TeamSelectionUI] Selected {abbr}");
    }

    List<TeamData> LoadTeamsFromStreamingAssets()
    {
        try
        {
            var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
            if (!File.Exists(path))
            {
                Debug.LogError($"[TeamSelectionUI] Missing {path}");
                return new List<TeamData>();
            }

            var json = File.ReadAllText(path).TrimStart();
            if (json.StartsWith("[")) json = "{\"teams\":" + json + "}";   // accept array or object root

            var list = JsonUtility.FromJson<TeamDataList>(json);
            return list?.teams ?? new List<TeamData>();
        }
        catch (Exception e)
        {
            Debug.LogError($"[TeamSelectionUI] Failed to parse teams.json: {e}");
            return new List<TeamData>();
        }
    }

    void AutoWireList()
    {
        var sr = GetComponentInChildren<ScrollRect>(true);
        if (sr && sr.content) listContent = sr.content;
    }
}
