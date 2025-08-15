using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GG.Game;

public class TeamSelectionUI : MonoBehaviour
{
    [Header("Left List")]
    [SerializeField] private RectTransform listContent;
    [SerializeField] private GameObject teamRowPrefab;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;

    [Header("Right Panel Preview")]
    [SerializeField] private RosterPanelUI preview;

    private string selectedAbbr = "";

    void Awake()
    {
        if (!preview) preview = FindFirstObjectByType<RosterPanelUI>(FindObjectsInactive.Include);
        if (confirmButton)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirm);
        }
    }

    void Start()
    {
        if (!listContent || !teamRowPrefab) AutoWireList();
        if (confirmButton) confirmButton.interactable = false;

        var teams = LoadTeamsFromStreamingAssets();
        if (teams == null || teams.Count == 0)
        {
            Debug.LogError("[TeamSelectionUI] No teams loaded from StreamingAssets/teams.json");
            return;
        }

        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);

        foreach (var t in teams.OrderBy(x => x.city).ThenBy(x => x.name))
        {
            var row = Instantiate(teamRowPrefab, listContent);
            var ui = row.GetComponent<TeamRowUI>();
            if (ui != null) ui.Set(t, () => OnTeamClicked(t.abbreviation));
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
        preview?.ShowRosterForTeam(abbr);
        Debug.Log($"[TeamSelectionUI] Selected {abbr}");
    }

    void OnConfirm()
    {
        if (string.IsNullOrEmpty(selectedAbbr))
        {
            Debug.LogWarning("[TeamSelectionUI] Confirm clicked with no team selected.");
            return;
        }

        GameState.SelectedTeamAbbr = selectedAbbr;
        PlayerPrefs.SetString("selected_team", selectedAbbr);
        PlayerPrefs.Save();

        Debug.Log($"[TeamSelectionUI] Confirm → Dashboard for {selectedAbbr}");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Dashboard");
    }

    List<TeamData> LoadTeamsFromStreamingAssets()
    {
        try
        {
            var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
            if (!File.Exists(path)) { Debug.LogError($"[TeamSelectionUI] Missing {path}"); return new(); }
            var json = File.ReadAllText(path).TrimStart();
            if (json.StartsWith("[")) json = "{\"teams\":" + json + "}";
            var list = JsonUtility.FromJson<TeamDataList>(json);
            return list?.teams ?? new();
        }
        catch (Exception e)
        {
            Debug.LogError($"[TeamSelectionUI] Failed to parse teams.json: {e}");
            return new();
        }
    }

    void AutoWireList()
    {
        var sr = GetComponentInChildren<ScrollRect>(true);
        if (sr && sr.content) listContent = sr.content;
    }
}
