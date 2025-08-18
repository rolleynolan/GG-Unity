using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class GGInfraGuard
{
    static readonly string[] CorePaths = {
        "Assets/Scripts/Core/GGPaths.cs",
        "Assets/Scripts/Core/GGLog.cs",
        "Assets/Scripts/Core/TeamProvider.cs"
    };

    static GGInfraGuard()
    {
        // 1) Warn if any asmdef sits under Assets/Scripts (we want single assembly)
        var asmdefs = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets/Scripts" });
        if (asmdefs.Length > 0)
        {
            var paths = asmdefs.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            Debug.LogWarning($"[GGInfraGuard] Detected asmdefs under Assets/Scripts (single-assembly expected):\n - {string.Join("\n - ", paths)}");
        }

        // 2) Warn if any of the core files are missing
        foreach (var p in CorePaths)
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(p) == null)
                Debug.LogError($"[GGInfraGuard] Missing required core file: {p}");
        }

        // 3) Warn if duplicate class files exist elsewhere (name-based scan)
        WarnIfDuplicate("GGPaths.cs",    "Assets/Scripts/Core/GGPaths.cs");
        WarnIfDuplicate("GGLog.cs",      "Assets/Scripts/Core/GGLog.cs");
        WarnIfDuplicate("TeamProvider.cs","Assets/Scripts/Core/TeamProvider.cs");

        // 4) Shim duplication check
        var shims = AssetDatabase.FindAssets("InfraCompat t:Script", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath).ToArray();
        if (shims.Length > 1)
            Debug.LogError($"[GGInfraGuard] Multiple InfraCompat shims found:\n - {string.Join("\n - ", shims)}\nKeep exactly one (Assets/Scripts/InfraCompat.cs).");
    }

    static void WarnIfDuplicate(string fileName, string expectedPath)
    {
        var hits = AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(fileName) + " t:Script")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(fileName) && p != expectedPath)
            .ToArray();

        if (hits.Length > 0)
            Debug.LogError($"[GGInfraGuard] Duplicate '{fileName}' detected (delete extras):\n - {string.Join("\n - ", hits)}\nExpected only: {expectedPath}");
    }
}
