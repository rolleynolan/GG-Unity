using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GG.Game;

[Serializable]
public class TeamData { public string city; public string name; public string conference; public string abbreviation; }
[Serializable]
public class TeamDataList { public List<TeamData> teams = new(); }

public class TeamSelectionUI : MonoBehaviour
{
    [Header("Left List")]
    [SerializeField] private Transform listContent;    // Scroll/Viewport/Content
    [SerializeField] private GameObject teamRowPrefab; // TeamRowUI prefab

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;     // ConfirmTeamButton

    private List<TeamData> _teams = new();

    void Awake()
    {
        // Auto-wire to survive lost references
        if (!confirmButton)
        {
            var go = GameObject.Find("ConfirmTeamButton");
            if (go) confirmButton = go.GetComponent<Button>();
        }
        if (!listContent)
        {
            var go = GameObject.Find("LeftPanel/Viewport/Content");
            if (go) listContent = go.transform;
        }
        // teamRowPrefab stays serialized; if null we will error clearly in Start
    }

    void Start()
    {
        if (!confirmButton) { Debug.LogError("[TeamSelectionUI] ConfirmButton is not assigned."); return; }
        if (!listContent)   { Debug.LogError("[TeamSelectionUI] List Content is not assigned.");   return; }
        if (!teamRowPrefab) { Debug.LogError("[TeamSelectionUI] Team Row Prefab is not assigned."); return; }

        confirmButton.interactable = false;
        if (!TryLoadTeams(out _teams)) return;
        PopulateTeamList();
    }

    bool TryLoadTeams(out List<TeamData> teams)
    {
        teams = new List<TeamData>();
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, "teams.json");
            if (!File.Exists(path)) { Debug.LogError($"[TeamSelectionUI] Missing teams.json at {path}"); return false; }
            string json = File.ReadAllText(path);
            if (json.TrimStart().StartsWith("[")) json = "{\"teams\":" + json + "}"; // allow array root
            var data = JsonUtility.FromJson<TeamDataList>(json);
            teams = data?.teams ?? new List<TeamData>();
            teams.Sort((a,b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            Debug.Log($"[TeamSelectionUI] Teams count: {teams.Count}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[TeamSelectionUI] Failed to load teams.json: {e}");
            return false;
        }
    }

    void PopulateTeamList()
    {
        for (int i = listContent.childCount - 1; i >= 0; i--) Destroy(listContent.GetChild(i).gameObject);
        foreach (var t in _teams)
        {
            var row = Instantiate(teamRowPrefab, listContent);
            var binder = row.GetComponent<TeamRowBinder>();
            if (!binder) binder = row.AddComponent<TeamRowBinder>();
            binder.Set(t);
            var btn = row.GetComponent<Button>();
            if (!btn) btn = row.AddComponent<Button>();
            btn.onClick.AddListener(() => OnTeamClicked(t));
        }
    }

    void OnTeamClicked(TeamData t)
    {
        GameState.SelectedTeamAbbr = t.abbreviation;
        confirmButton.interactable = true;
        Debug.Log($"[TeamSelectionUI] Selected {t.abbreviation}");
    }

    public void OnConfirm()
    {
        if (string.IsNullOrEmpty(GameState.SelectedTeamAbbr)) return;
        SceneManager.LoadScene("Dashboard");
    }
}
