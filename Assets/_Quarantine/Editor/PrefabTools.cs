#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PrefabTools
{
    [MenuItem("GridironGM/Dev/Scan & Remove Missing Scripts")]
    public static void RemoveMissing()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int removed = 0, scanned = 0;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) continue;
            scanned++;
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
        }
        GGLog.Info($"Prefabs scanned:{scanned}, missing scripts removed:{removed}");
    }

    [MenuItem("GridironGM/Dev/Clear Season Save")]
    public static void ClearSeason()
    {
        var p = GGPaths.Save(GGConventions.SeasonSaveFile);
        if (File.Exists(p))
        {
            File.Delete(p);
            GGLog.Info($"Deleted {p}");
        }
        else
        {
            GGLog.Info("No season save found.");
        }
    }
}
#endif
