using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface ITeamProvider { List<string> GetAllTeamAbbrs(); }

[System.Serializable] class _TP_Team { public string abbreviation; }
[System.Serializable] class _TP_Root { public _TP_Team[] teams; }

public sealed class TeamProvider : ITeamProvider
{
    public List<string> GetAllTeamAbbrs()
    {
        var list = new List<string>();
        try
        {
            var path = GGPaths.Streaming(GGConventions.TeamsJsonFile);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var root = JsonUtility.FromJson<_TP_Root>(json);
                if (root?.teams != null)
                    foreach (var t in root.teams)
                        if (!string.IsNullOrEmpty(t.abbreviation))
                            list.Add(t.abbreviation);
            }
        }
        catch (System.Exception ex)
        {
            GGLog.Warn($"TeamProvider load failed ({ex.Message}).");
        }

        if (list.Count == 0)
        {
            list.AddRange(new[] { "ATL", "PHI", "DAL", "NYG" });
            GGLog.Warn("TeamProvider: falling back to default ABBRs.");
        }
        else
        {
            GGLog.Info($"TeamProvider: loaded {list.Count} team ABBRs.");
        }
        return list;
    }
}
