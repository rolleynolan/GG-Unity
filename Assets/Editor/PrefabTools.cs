using System.IO;
using UnityEditor;

public static class PrefabTools
{
    [MenuItem("Tools/GG/Scan Missing Scripts")]
    public static void ScanMissingScripts()
    {
        GGCleanupTools.CleanupMissingInOpenScenes();
        GGCleanupTools.CleanupMissingInPrefabs();
    }

    [MenuItem("Tools/GG/Clear Season Save")]
    public static void ClearSeasonSave()
    {
        var path = GGPaths.Save(GGConventions.SeasonSaveFile);
        if (File.Exists(path))
        {
            File.Delete(path);
            GGLog.Info($"Deleted season save at {path}");
        }
        else
        {
            GGLog.Warn($"No season save found at {path}");
        }
    }
}
