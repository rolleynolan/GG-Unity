using System.IO;
using UnityEditor;
using UnityEngine;

public static class SeasonTools
{
    [MenuItem("GridironGM/Dev/Clear Season Save")]
    public static void ClearSeason()
    {
        var path = Path.Combine(Application.persistentDataPath, "season.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SeasonTools] Deleted season save: {path}");
        }
        else
        {
            Debug.Log("[SeasonTools] No season save found.");
        }
    }
}
