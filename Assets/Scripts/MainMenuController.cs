using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;
using Debug = UnityEngine.Debug;     // <— disambiguate Debug
using Diag = System.Diagnostics;     // <— alias Diagnostics
using GridironGM.Data.Legacy;  // where LeagueState/TeamRosterEntry live now



public class MainMenuController : MonoBehaviour
{
    [Header("GM Settings")]
    public Dropdown gmDropdown;
    public InputField gmNameInput;
    public Button createGmButton;

    [Header("League Settings")]
    public Dropdown teamDropdown;
    public Button simulateWeekButton;
    public TMP_Text simStatusText;

    [Header("Roster UI")]
    public GameObject playerRowPrefab;
    public Transform rosterContent;

    private LeagueState leagueState;
    private string leagueStatePath;

    void Start()
    {
        leagueStatePath = Path.Combine(Application.dataPath, "..", "save", "league_state.json");
        LoadLeagueState();
        PopulateTeamDropdown();
        PopulateGmDropdown();

        if (teamDropdown != null)
            teamDropdown.onValueChanged.AddListener(delegate { OnTeamSelected(); });

        if (leagueState != null)
            OnTeamSelected();

        if (createGmButton != null)
            createGmButton.onClick.AddListener(CreateGm);

        if (simulateWeekButton != null)
            simulateWeekButton.onClick.AddListener(RunSimulateWeek);
    }

    void LoadLeagueState()
    {
        if (!File.Exists(leagueStatePath))
        {
            Debug.LogError($"League state file not found at {leagueStatePath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(leagueStatePath);
            leagueState = JsonUtility.FromJson<LeagueStateWrapper>($"{{\"leagueState\":{json}}}").leagueState;
            Debug.Log($"Loaded league state with {leagueState.teams.Count} teams and {leagueState.free_agents.Count} free agents.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse league state: {ex.Message}");
        }
    }

    void PopulateTeamDropdown()
    {
        if (teamDropdown == null || leagueState == null) return;

        teamDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var team in leagueState.teams)
            options.Add(team.team);

        teamDropdown.AddOptions(options);
    }

    void PopulateGmDropdown()
    {
        if (gmDropdown == null) return;

        gmDropdown.ClearOptions();
        var gmDir = Path.Combine(Application.dataPath, "..", "gms");
        var options = new List<string>();

        if (Directory.Exists(gmDir))
        {
            foreach (var f in Directory.GetFiles(gmDir, "*.json"))
                options.Add(Path.GetFileNameWithoutExtension(f));
        }

        gmDropdown.AddOptions(options);
    }

    void OnTeamSelected()
    {
        if (teamDropdown == null || leagueState == null) return;
        if (teamDropdown.value < 0 || teamDropdown.value >= leagueState.teams.Count) return;

        var team = leagueState.teams[teamDropdown.value];
        PopulateTeamRoster(team);
    }

    void PopulateTeamRoster(TeamRosterEntry selectedTeam)
    {
        if (selectedTeam == null || rosterContent == null || playerRowPrefab == null)
            return;

        foreach (Transform child in rosterContent)
            Destroy(child.gameObject);

        if (selectedTeam.players != null)
        {
            foreach (var player in selectedTeam.players)
            {
                var rowObj = Instantiate(playerRowPrefab, rosterContent);
                var row = rowObj.GetComponent<PlayerRow>();
                if (row != null)
                    row.SetData(player);
            }
        }
    }

    void CreateGm()
    {
        if (gmNameInput == null) return;

        var name = gmNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) return;

        var gmDir = Path.Combine(Application.dataPath, "..", "gms");
        Directory.CreateDirectory(gmDir);

        var path = Path.Combine(gmDir, name + ".json");
        if (!File.Exists(path))
            File.WriteAllText(path, "{}");

        PopulateGmDropdown();
        int index = gmDropdown.options.FindIndex(o => o.text == name);
        if (index >= 0)
            gmDropdown.value = index;
    }

    void RunSimulateWeek()
    {
        var process = new Diag.Process();
        process.StartInfo.FileName = "python";
        process.StartInfo.WorkingDirectory = Path.Combine(Application.dataPath, "..", "..");
        process.StartInfo.Arguments = "scripts/run_weekly_simulation.py";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        if (simulateWeekButton != null) simulateWeekButton.interactable = false;
        if (simStatusText != null)      simStatusText.text = "Simulating...";

        try
        {
            process.Start();
            process.WaitForExit();

            Debug.Log(process.StandardOutput.ReadToEnd());
            Debug.LogError(process.StandardError.ReadToEnd());

            if (simStatusText != null) simStatusText.text = "Simulation complete";
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            if (simStatusText != null) simStatusText.text = "Simulation failed";
        }
        finally
        {
            if (simulateWeekButton != null) simulateWeekButton.interactable = true;
        }

        LoadLeagueState();
        OnTeamSelected();
    }
}
