#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectCleanup
{
    [MenuItem("GridironGM/Cleanup/Remove Missing Scripts In Scene")]
    public static void RemoveInScene()
    {
        int total = 0;
        foreach (var go in Object.FindObjectsOfType<GameObject>())
            total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        Debug.Log($"[Cleanup] Removed {total} missing scripts from OPEN scene.");
    }

    [MenuItem("GridironGM/Cleanup/Remove Missing Scripts In All Scenes")]
    public static void RemoveInAllScenes()
    {
        var guids = AssetDatabase.FindAssets("t:Scene");
        int scenes = 0, total = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int removed = 0;
            foreach (var go in scene.GetRootGameObjects())
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                scenes++;
                total += removed;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
        Debug.Log($"[Cleanup] Removed {total} missing scripts across {scenes} scenes.");
    }

    [MenuItem("GridironGM/Cleanup/Remove Missing Scripts In Prefabs")]
    public static void RemoveInPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int prefabs = 0, total = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!go) continue;
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                total += removed;
                prefabs++;
                EditorUtility.SetDirty(go);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[Cleanup] Removed {total} missing scripts across {prefabs} prefabs.");
    }
}
#endif
