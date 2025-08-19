// 8/19/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEditor;
using UnityEngine;

public static class PrefabTools
{
    public static void ProcessPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            GGLog.Warning("The provided prefab is null.");
            return;
        }

        string jsonPath = GGPaths.Json; // Corrected usage of GGPaths.Json as a property
        GGLog.Info($"Processing prefab '{prefab.name}' using JSON path: {jsonPath}");

        // Your prefab processing logic here
    }
}