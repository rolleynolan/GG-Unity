#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectCleanup
{
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
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) continue;
            int removed = 0;
            foreach (var comp in prefab.GetComponentsInChildren<Component>(true))
                if (comp == null) removed++;
            if (removed > 0)
            {
                var stage = PrefabUtility.LoadPrefabContents(path);
                foreach (var go in stage.GetComponentsInChildren<GameObject>(true))
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                PrefabUtility.SaveAsPrefabAsset(stage, path);
                PrefabUtility.UnloadPrefabContents(stage);
                total += removed;
            }
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
            total += Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Sum(go => GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go));
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log($"[Cleanup] Removed {total} missing script components across all scenes.");
    }
}
#endif
