#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptUtility
{
    const string AutoKey = "GG.Cleanup.AutoRunOnPlay";

    [MenuItem("GridironGM/Cleanup/Remove Missing Scripts In Open Scenes")]
    public static void CleanOpenScenes()
    {
        int total = 0, gos = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            foreach (var root in s.GetRootGameObjects())
            {
                total += RemoveMissingRecursive(root, ref gos);
            }
            EditorSceneManager.MarkSceneDirty(s);
        }
        Debug.Log($"[Cleanup] Open scenes: removed {total} missing script components on {gos} objects.");
    }

    [MenuItem("GridironGM/Cleanup/Remove Missing Scripts In Project (All Prefabs)")]
    public static void CleanAllPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int total = 0, gos = 0, touched = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Cleaning Prefabs", path, (float)i / guids.Length);

                var root = PrefabUtility.LoadPrefabContents(path);
                int beforeTotal = total, beforeGos = gos;

                total += RemoveMissingRecursive(root, ref gos);

                if (total != beforeTotal || gos != beforeGos)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    touched++;
                }
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log($"[Cleanup] Prefabs: removed {total} missing script components on {gos} objects across {touched} modified prefabs.");
    }

    [MenuItem("GridironGM/Cleanup/Remove Missing Scripts (Scenes + Prefabs)")]
    public static void CleanAll()
    {
        CleanOpenScenes();
        CleanAllPrefabs();
    }

    [MenuItem("GridironGM/Cleanup/Toggle Auto-Clean On Play")]
    public static void ToggleAuto()
    {
        bool cur = EditorPrefs.GetBool(AutoKey, false);
        EditorPrefs.SetBool(AutoKey, !cur);
        Debug.Log($"[Cleanup] Auto-clean on Play: {(!cur ? "ENABLED" : "DISABLED")}");
    }

    // ---------- Internals ----------

    static int RemoveMissingRecursive(GameObject root, ref int objectCount)
    {
        int removed = 0;
        var stack = new Stack<Transform>();
        stack.Push(root.transform);

        while (stack.Count > 0)
        {
            var t = stack.Pop();
            foreach (Transform c in t) stack.Push(c);

            // Unity has a helper for this in the Editor:
            int before = CountMissingOn(t.gameObject);
            if (before > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                removed += before;
                objectCount++;
            }
        }
        return removed;
    }

    static int CountMissingOn(GameObject go)
    {
        int n = 0;
        var comps = go.GetComponents<Component>();
        foreach (var c in comps) if (c == null) n++;
        return n;
    }

    // Auto-run on Play once (Editor only)
    [InitializeOnLoadMethod]
    static void HookPlayMode()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (!EditorPrefs.GetBool(AutoKey, false)) return;
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Debug.Log("[Cleanup] Auto-clean before Play…");
                CleanAll();
                AssetDatabase.SaveAssets();
            }
        };
    }
}
#endif
