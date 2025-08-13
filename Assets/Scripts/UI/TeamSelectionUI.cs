using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GG.Game;

[Serializable]
public class TeamData
{
    public string city;
    public string name;
    public string conference;
    public string abbreviation;
}

[Serializable]
public class TeamDataList
{
    public List<TeamData> teams = new();
}

public class TeamSelectionUI : MonoBehaviour
{
    [Header("Left List")]
    [SerializeField] private Transform listContent;      // Scroll/Viewport/Content for team rows
    [SerializeField] private GameObject teamRowPrefab;   // TeamRowUI prefab with TMP texts

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;

    private List<TeamData> _teams;

    void Start()
    {
        confirmButton.interactable = false;
        LoadTeams();
        PopulateTeamList();
    }

    void LoadTeams()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "teams.json");
        string json = File.ReadAllText(path);
        if (json.TrimStart().StartsWith("[")) json = "{\"teams\":" + json + "}"; // allow array root
        var data = JsonUtility.FromJson<TeamDataList>(json);
        _teams = data?.teams ?? new List<TeamData>();
        _teams.Sort((a,b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        Debug.Log($"[TeamSelectionUI] Teams count: {_teams.Count}");
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
