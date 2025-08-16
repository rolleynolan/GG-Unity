using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable] class __TP_Team { public string city; public string name; public string abbreviation; }
[System.Serializable] class __TP_Root { public __TP_Team[] teams; }

public static class TeamProvider
{
    public static List<string> GetAbbrs()
    {
        var list = new List<string>();
        try
        {
            var path = Path.Combine(Application.streamingAssetsPath, "teams.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var root = JsonUtility.FromJson<__TP_Root>(json);
                if (root?.teams != null)
                    foreach (var t in root.teams)
                        if (!string.IsNullOrEmpty(t.abbreviation))
                            list.Add(t.abbreviation);
            }
        }
        catch { }
        if (list.Count == 0) list.AddRange(new[] { "ATL", "PHI", "DAL", "NYG" });
        return list;
    }
}
