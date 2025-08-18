using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RosterBootStrapper : MonoBehaviour
{
    [Tooltip("If enabled, ensures placeholder roster/schedule JSON exist in persistentDataPath on Play.")]
    [SerializeField] private bool generateOnPlay = false;

    void Start()
    {
        if (generateOnPlay) EnsureFiles();
    }

    private void EnsureFiles()
    {
        string basePath = Application.persistentDataPath;
        string rostersPath = Path.Combine(basePath, "rosters_by_team.json");
        string schedulePath = Path.Combine(basePath, "schedule_by_team.json");

        // Teams source (Resources/Config/teams.json or persistent)
        var teams = LeagueRepository.GetTeams();
        if (teams == null || teams.Length == 0)
        {
            Debug.LogWarning("[RosterBootStrapper] No teams found; cannot seed files.");
            return;
        }

        if (!File.Exists(rostersPath))
        {
            var dict = new Dictionary<string, List<Player>>();
            foreach (var t in teams)
            {
                var list = new List<Player>();
                for (int i = 0; i < 12; i++)
                {
                    list.Add(new Player
                    {
                        id = System.Guid.NewGuid().ToString("N"),
                        name = $"{t.abbreviation} Player {i + 1}",
                        position = i % 3 == 0 ? "WR" : (i % 3 == 1 ? "RB" : "CB"),
                        overall = 68 + (i % 8),
                        number = 10 + i
                    });
                }
                dict[t.abbreviation] = list;
            }
            File.WriteAllText(rostersPath, ToJsonDict(dict));
            Debug.Log($"[RosterBootStrapper] Wrote {rostersPath}");
        }

        if (!File.Exists(schedulePath))
        {
            var dict = new Dictionary<string, List<TeamGame>>();
            foreach (var t in teams)
            {
                var list = new List<TeamGame>();
                for (int w = 1; w <= 18; w++)
                {
                    list.Add(new TeamGame
                    {
                        week = w,
                        opponent = "TBD",
                        home = (w % 2 == 0),
                        time = "1:00 PM",
                        result = ""
                    });
                }
                dict[t.abbreviation] = list;
            }
            File.WriteAllText(schedulePath, ToJsonDict(dict));
            Debug.Log($"[RosterBootStrapper] Wrote {schedulePath}");
        }
    }

    // Manual JSON writer to match { "ATL":[...], "BUF":[...], ... }
    private string ToJsonDict<T>(Dictionary<string, List<T>> dict)
    {
        var entries = new System.Text.StringBuilder();
        entries.Append('{');
        bool first = true;
        foreach (var kv in dict)
        {
            if (!first) entries.Append(',');
            first = false;
            string arr = JsonUtility.ToJson(new Wrapper<T> { items = kv.Value.ToArray() });
            int s = arr.IndexOf('['); int e = arr.LastIndexOf(']');
            string inner = (s >= 0 && e >= s) ? arr.Substring(s, e - s + 1) : "[]";
            entries.Append('\"').Append(kv.Key).Append("\":").Append(inner);
        }
        entries.Append('}');
        return entries.ToString();
    }

    [System.Serializable] private class Wrapper<T> { public T[] items; }
}
