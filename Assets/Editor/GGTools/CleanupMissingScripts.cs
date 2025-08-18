using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GGCleanupTools
{
    [MenuItem("Tools/GG/Cleanup Missing Scripts In Scene")]
    public static void CleanupMissingInOpenScenes()
    {
        int total = 0;
        for (int s = 0; s < SceneManager.sceneCount; s++)
        {
            var scene = SceneManager.GetSceneAt(s);
            foreach (var root in scene.GetRootGameObjects())
            {
                total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            }
            EditorSceneManager.MarkSceneDirty(scene);
        }
        GGLog.Info($"[GGCleanup] Removed {total} missing script components from open scene(s).");
    }

    [MenuItem("Tools/GG/Cleanup Missing Scripts In Project Prefabs")]
    public static void CleanupMissingInPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int total = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!go)
            {
                continue;
            }
            total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            EditorUtility.SetDirty(go);
        }
        AssetDatabase.SaveAssets();
        GGLog.Info($"[GGCleanup] Removed {total} missing script components from prefabs.");
    }
}
