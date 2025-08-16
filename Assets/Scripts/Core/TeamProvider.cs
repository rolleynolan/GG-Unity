using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface ITeamProvider
{
    List<string> GetAllTeamAbbrs();
}

public class TeamProvider : ITeamProvider
{
    [Serializable] private class Team { public string abbreviation; }
    [Serializable] private class Root { public Team[] teams; }

    public List<string> GetAllTeamAbbrs()
    {
        var list = new List<string>();
        var path = GGPaths.Streaming(GGConventions.TeamsJsonFile);

        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var root = JsonUtility.FromJson<Root>(json);
                if (root?.teams != null)
                {
                    foreach (var t in root.teams)
                    {
                        if (!string.IsNullOrEmpty(t.abbreviation))
                        {
                            list.Add(t.abbreviation);
                        }
                    }
                }
            }

            if (list.Count > 0)
            {
                GGLog.Info($"TeamProvider loaded {list.Count} teams from {path}.");
            }
            else
            {
                GGLog.Warn("TeamProvider found no team abbreviations; using defaults.");
                list.AddRange(new[] { "ATL", "PHI", "DAL", "NYG" });
            }
        }
        catch (Exception ex)
        {
            GGLog.Error($"TeamProvider failed to read teams from {path}", ex);
            list.AddRange(new[] { "ATL", "PHI", "DAL", "NYG" });
        }

        return list;
    }
}
