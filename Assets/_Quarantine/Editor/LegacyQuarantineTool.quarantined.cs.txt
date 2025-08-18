#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class LegacyQuarantineTool
{
    const string SrcFolder = "Assets/Gridiron GM Alpha Build";
    const string LegacyRoot = "Assets/_Legacy";
    const string AsmdefPath = LegacyRoot + "/LegacyAlpha.asmdef";
    const string Flag = "GG.Legacy.Quarantined";

    [InitializeOnLoadMethod]
    static void AutoRunOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorPrefs.GetBool(Flag, false)) return;
        if (!AssetDatabase.IsValidFolder(SrcFolder)) return;

        if (!EditorUtility.DisplayDialog("Quarantine Legacy Alpha Code?",
            "Move 'Gridiron GM Alpha Build' under Assets/_Legacy and make it Editor-only?\n\n" +
            "This is reversible and prevents legacy scripts from compiling into the game.", "Quarantine", "Skip"))
            return;

        Quarantine();
        EditorPrefs.SetBool(Flag, true);
    }

    [MenuItem("GridironGM/Legacy/Quarantine Old Alpha Code (move & asmdef)")]
    public static void Quarantine()
    {
        if (!AssetDatabase.IsValidFolder(SrcFolder))
        {
            Debug.Log("[LegacyQuarantine] Nothing to move.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(LegacyRoot))
            AssetDatabase.CreateFolder("Assets", "_Legacy");

        var dst = LegacyRoot + "/Gridiron GM Alpha Build";
        var res = AssetDatabase.MoveAsset(SrcFolder, dst);
        if (!string.IsNullOrEmpty(res))
        {
            Debug.LogWarning($"[LegacyQuarantine] Move failed: {res}");
            return;
        }

        // Create Editor-only asmdef
        var json = "{\n  \"name\": \"LegacyAlpha\",\n  \"includePlatforms\": [\"Editor\"]\n}\n";
        File.WriteAllText(AsmdefPath, json);
        AssetDatabase.ImportAsset(AsmdefPath);

        Debug.Log($"[LegacyQuarantine] Moved to '{dst}' and created Editor-only asmdef.");
    }
}
#endif
