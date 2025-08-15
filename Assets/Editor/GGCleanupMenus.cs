#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Safe, non-destructive cleanup menus.
/// Appears under: Tools → GG Cleanup → Run (Safe)
///            and: GridironGM → Cleanup → Run (Safe)
public static class GGCleanupMenus
{
    // --------------------------------------------------------------------
    // MENUS
    // --------------------------------------------------------------------
    [MenuItem("Tools/GG Cleanup/Run (Safe)")]
    [MenuItem("GridironGM/Cleanup/Run (Safe)")]
    public static void RunSafeCleanup()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("GG Cleanup", "Exit Play Mode first.", "OK");
            return;
        }

        int missScenes   = CleanOpenScenes_MissingScripts(out int missSceneObjs);
        int missPrefabs  = CleanAllPrefabs_MissingScripts(out int missPrefabObjs, out int missTouched);

        int tmpScenes    = FixOpenScenes_TMPCanvasRenderer(out int tmpSceneObjs);
        int tmpPrefabs   = FixAllPrefabs_TMPCanvasRenderer(out int tmpPrefabObjs, out int tmpTouched);

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "GG Cleanup (Safe)",
            $"Scenes: removed {missScenes} missing scripts on {missSceneObjs} objects\n" +
            $"Prefabs: removed {missPrefabs} missing scripts on {missPrefabObjs} objects (modified {missTouched})\n\n" +
            $"Scenes: removed {tmpScenes} TMP CanvasRenderers on {tmpSceneObjs} objects\n" +
            $"Prefabs: removed {tmpPrefabs} TMP CanvasRenderers on {tmpPrefabObjs} objects (modified {tmpTouched})",
            "OK"
        );
    }

    [MenuItem("Tools/GG Cleanup/Smoke Test")]
    public static void Smoke()
    {
        EditorUtility.DisplayDialog("GG Menu", "Menus are active and compiling.", "OK");
    }

    // --------------------------------------------------------------------
    // PASS 1: Remove “missing script” components (null MonoBehaviours)
    // --------------------------------------------------------------------
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

            // Count null components on this GO
            int nullCount = 0;
            foreach (var comp in t.GetComponents<Component>())
                if (comp == null) nullCount++;

            if (nullCount > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                removed   += nullCount;
                objectCount++;
            }
        }
        return removed;
    }

    // --------------------------------------------------------------------
    // PASS 2: Remove illegal CanvasRenderer on 3D TextMeshPro (not UGUI)
    // TMP logs: “Please remove the CanvasRenderer component … no longer necessary.”
    // --------------------------------------------------------------------
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

            // Use reflection so we don’t hard-depend on TMP compile symbols
            var tmp3D   = t.GetComponent("TMPro.TextMeshPro");        // 3D TextMeshPro
            var tmpUGUI = t.GetComponent("TMPro.TextMeshProUGUI");    // UGUI version

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
