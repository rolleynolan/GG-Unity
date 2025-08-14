#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectCleanup
{
    [MenuItem("GridironGM/Cleanup/Apply Project Patch (delete legacy + cleanup)")]
    public static void ApplyPatch()
    {
        string[] toDelete =
        {
            "Assets/Scripts/RosterUI.cs",
            "Assets/Scripts/PlayerRow.cs",
            "Assets/Scripts/TeamDataUI.cs" // delete only if it exists; safe if missing
        };

        foreach (var path in toDelete)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[Patch] Deleted legacy asset: {path}");
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RemoveMissingInPrefabs();
        RemoveMissingInAllScenes();

        Debug.Log("[Patch] Project patch complete. If you see compile errors, re-import and play again.");
    }

    [MenuItem("GridironGM/Cleanup/Remove Missing Scripts In Scene")]
    public static void RemoveMissingInScene()
    {
        int count = 0;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        Debug.Log($"[Cleanup] Removed {count} missing script components in current scene.");
    }

    [MenuItem("GridironGM/Cleanup/Remove Missing Scripts In Prefabs")]
    public static void RemoveMissingInPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int total = 0;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var stage = PrefabUtility.LoadPrefabContents(path);
            int removed = 0;
            foreach (var go in stage.GetComponentsInChildren<GameObject>(true))
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(stage, path);
                total += removed;
            }
            PrefabUtility.UnloadPrefabContents(stage);
        }
        Debug.Log($"[Cleanup] Removed ~{total} missing script components across prefabs.");
    }

    [MenuItem("GridironGM/Cleanup/Remove Missing Scripts In All Scenes")]
    public static void RemoveMissingInAllScenes()
    {
        var sceneGuids = AssetDatabase.FindAssets("t:Scene");
        int total = 0;
        foreach (var g in sceneGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log($"[Cleanup] Removed {total} missing script components across all scenes.");
    }
}
#endif
