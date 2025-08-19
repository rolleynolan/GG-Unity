#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

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
        // Build the path directly, no GGPaths/GGLog dependency.
        var path = Path.Combine(Application.persistentDataPath, "GGData", GGConventions.SeasonSaveFile);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[GG] Deleted season save at {path}");
        }
        else
        {
            Debug.LogWarning($"[GG] No season save found at {path}");
        }
    }
}
#endif
