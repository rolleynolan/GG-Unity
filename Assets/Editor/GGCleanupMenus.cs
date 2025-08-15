#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Safe, non-breaking cleanup menu. No deletions; only removes known-problem components.
public static class GGCleanupMenus
{
    // Two menu entries so it's easy to find.
    [MenuItem("Tools/GG Cleanup/Run (Safe)")]
    [MenuItem("GridironGM/Cleanup/Run (Safe)")]
    public static void RunSafeCleanup()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("GG Cleanup", "Exit Play Mode first.", "OK");
            return;
        }

        int missScenes = CleanOpenScenes_MissingScripts(out int missSceneObjs);
        int missPrefabs = CleanAllPrefabs_MissingScripts(out int missPrefabObjs, out int touchedPrefabs);

        int tmpScenes = FixOpenScenes_TMPCanvasRenderer(out int tmpSceneObjs);
        int tmpPrefabs = FixAllPrefabs_TMPCanvasRenderer(out int tmpPrefabObjs, out int touchedTmpPrefabs);

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "GG Cleanup (Safe)",
            $"Scenes: removed {missScenes} missing scripts on {missSceneObjs} objects\n" +
            $"Prefabs: removed {missPrefabs} missing scripts on {missPrefabObjs} objects (modified {touchedPrefabs})\n\n" +
            $"Scenes: removed {tmpScenes} TMP CanvasRenderers on {tmpSceneObjs} objects\n" +
            $"Prefabs: removed {tmpPrefabs} TMP CanvasRenderers on {tmpPrefabObjs} objects (modified {touchedTmpPrefabs})",
            "OK"
        );
    }

    [MenuItem("Tools/GG Cleanup/Smoke Test")]
    public static void Smoke() =>
        EditorUtility.DisplayDialog("GG Menu", "Menus are active and compiling.", "OK");

    // ------- Missing scripts (scenes) -------
    static int CleanOpenScenes_MissingScripts(out int objectCount)
    {
        int total = 0; objectCount = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            foreach (var root in s.GetRootGameObjects())
                total += RemoveMissingRecursive(root, ref objectCount);
            if (total > 0) EditorSceneManager.MarkSceneDirty(s);
        }
        return total;
    }

    // ------- Missing scripts (prefabs) -------
    static int CleanAllPrefabs_MissingScripts(out int objectCount, out int touched)
    {
        objectCount = 0; touched = 0; int total = 0;
        var guids = AssetDatabase.FindAssets("t:Prefab");
        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Cleaning Prefabs (Missing Scripts)", path, (float)i / guids.Length);

                var root = PrefabUtility.LoadPrefabContents(path);
                int beforeTotal = total, beforeObjs = objectCount;

                total += RemoveMissingRecursive(root, ref objectCount);

                if (total != beforeTotal || objectCount != beforeObjs)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    touched++;
                }
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
        finally { EditorUtility.ClearProgressBar(); }
        return total;
    }

    static int RemoveMissingRecursive(GameObject root, ref int objectCount)
    {
        int removed = 0;
        var stack = new Stack<Transform>();
        stack.Push(root.transform);
        while (stack.Count > 0)
        {
            var t = stack.Pop();
            foreach (Transform c in t) stack.Push(c);

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
        int n = 0; foreach (var c in go.GetComponents<Component>()) if (c == null) n++;
        return n;
    }

    // ------- TMP CanvasRenderer fixes -------
    // Remove CanvasRenderer when object has TextMeshPro (3D) but NOT TextMeshProUGUI
    static int FixOpenScenes_TMPCanvasRenderer(out int objectCount)
    {
        int total = 0; objectCount = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            foreach (var root in s.GetRootGameObjects())
                total += FixTMPCanvasRenderersRecursive(root, ref objectCount);
            if (total > 0) EditorSceneManager.MarkSceneDirty(s);
        }
        return total;
    }

    static int FixAllPrefabs_TMPCanvasRenderer(out int objectCount, out int touched)
    {
        objectCount = 0; touched = 0; int total = 0;
        var guids = AssetDatabase.FindAssets("t:Prefab");
        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Cleaning Prefabs (TMP CanvasRenderer)", path, (float)i / guids.Length);

                var root = PrefabUtility.LoadPrefabContents(path);
                int beforeTotal = total, beforeObjs = objectCount;

                total += FixTMPCanvasRenderersRecursive(root, ref objectCount);

                if (total != beforeTotal || objectCount != beforeObjs)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    touched++;
                }
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
        finally { EditorUtility.ClearProgressBar(); }
        return total;
    }

    static int FixTMPCanvasRenderersRecursive(GameObject root, ref int objectCount)
    {
        int removed = 0;
        var stack = new Stack<Transform>();
        stack.Push(root.transform);
        while (stack.Count > 0)
        {
            var t = stack.Pop();
            foreach (Transform c in t) stack.Push(c);

            var tmp3D   = t.GetComponent("TMPro.TextMeshPro");        // 3D
            var tmpUGUI = t.GetComponent("TMPro.TextMeshProUGUI");    // UGUI
            if (tmp3D != null && tmpUGUI == null)
            {
                var cr = t.GetComponent<CanvasRenderer>();
                if (cr != null)
                {
                    Object.DestroyImmediate(cr, true);
                    removed++;
                    objectCount++;
                }
            }
        }
        return removed;
    }
}
#endif
