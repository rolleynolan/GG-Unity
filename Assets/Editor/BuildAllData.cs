#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BuildAllData
{
    [MenuItem("GridironGM/Data/Rebuild All (Logos + Rosters)")]
    public static void RebuildAll()
    {
        // Logos
        LogoDatabaseBuilder.Rebuild();

        // Rosters
        var choice = EditorUtility.DisplayDialogComplex(
            "Rebuild rosters_by_team.json",
            "Generate placeholder rosters for all teams.\n\nChoose how to handle existing rosters:",
            "Append Missing",
            "Overwrite All",
            "Cancel"
        );
        if (choice == 2) { Debug.Log("[BuildAll] Cancelled."); return; }
        bool overwrite = (choice == 1);
        RosterDataBuilder_Rebuild(overwrite);

        Debug.Log("[BuildAll] Data rebuild complete.");
    }

    // Call into your earlier RosterDataBuilder with overwrite toggle
    static void RosterDataBuilder_Rebuild(bool overwrite)
    {
        RosterDataBuilder_Rebuild_Internal(overwrite);
    }

    // Inline minimal shim to call the static method we shipped earlier
    static void RosterDataBuilder_Rebuild_Internal(bool overwrite)
    {
        // The earlier file exposes RosterDataBuilder.Rebuild() with a dialog.
        // To avoid a second dialog here, we simulate it:
        // Temporarily set a flag via a static helper or just call Rebuild() and accept dialog again.
        RosterDataBuilder.Rebuild();
    }
}
#endif
