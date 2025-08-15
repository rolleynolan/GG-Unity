#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GG.EditorTools
{
    public static class GGQuickCleanup
    {
        [MenuItem("GridironGM/Cleanup/Quick Non-Breaking Cleanup")]
        public static void RunAll()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            AssetDatabase.SaveAssets();

            int missScenes = CleanOpenScenes_MissingScripts(out int missSceneObjs);
            int missPrefabs = CleanAllPrefabs_MissingScripts(out int missPrefabObjs, out int prefabTouched1);

            int tmpScenes = FixOpenScenes_TMPCanvasRenderer(out int tmpSceneObjs);
            int tmpPrefabs = FixAllPrefabs_TMPCanvasRenderer(out int tmpPrefabObjs, out int prefabTouched2);

            // Optional: verify Dashboard binder has references (no changes if already wired)
            int dashAuto = AutoWireDashboardIfPresent();

            // Produce an unused assets *report only* (no deletes)
            var report = ReportUnusedAssets();

            AssetDatabase.SaveAssets();
            sw.Stop();

            var msg =
$@"✅ GG Quick Cleanup finished in {sw.Elapsed.TotalSeconds:0.0}s

• Missing scripts removed: scenes {missScenes} on {missSceneObjs} objects; prefabs {missPrefabs} on {missPrefabObjs} objects (touched {prefabTouched1} prefabs).
• TMP 3D CanvasRenderer removed: scenes {tmpScenes} on {tmpSceneObjs} objects; prefabs {tmpPrefabs} on {tmpPrefabObjs} objects (touched {prefabTouched2} prefabs).
• Dashboard auto-wire attempts: {dashAuto}.
• Unused assets report: {report}";

            Debug.Log(msg);
            EditorUtility.DisplayDialog("GG Quick Cleanup", msg, "OK");
        }

        // ---------- Missing scripts (scenes) ----------
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
            Debug.Log($"[GGCleanup] Scenes: removed {total} missing scripts on {objectCount} objects.");
            return total;
        }

        // ---------- Missing scripts (prefabs) ----------
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
            Debug.Log($"[GGCleanup] Prefabs: removed {total} missing scripts on {objectCount} objects (touched {touched}).");
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
            int n = 0;
            var comps = go.GetComponents<Component>();
            foreach (var c in comps) if (c == null) n++;
            return n;
        }

        // ---------- TMP CanvasRenderer (scenes) ----------
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
            Debug.Log($"[GGCleanup] Scenes: removed {total} TMP CanvasRenderer warnings on {objectCount} objects.");
            return total;
        }

        // ---------- TMP CanvasRenderer (prefabs) ----------
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
            Debug.Log($"[GGCleanup] Prefabs: removed {total} TMP CanvasRenderer warnings on {objectCount} objects (touched {touched}).");
            return total;
        }

        // Remove CanvasRenderer when object has TextMeshPro (3D) but not TextMeshProUGUI
        static int FixTMPCanvasRenderersRecursive(GameObject root, ref int objectCount)
        {
            int removed = 0;
            var stack = new Stack<Transform>();
            stack.Push(root.transform);
            while (stack.Count > 0)
            {
                var t = stack.Pop();
                foreach (Transform c in t) stack.Push(c);

                var tmp3D = t.GetComponent("TMPro.TextMeshPro");         // 3D
                var tmpUI = t.GetComponent("TMPro.TextMeshProUGUI");     // UGUI
                if (tmp3D != null && tmpUI == null)
                {
                    var cr = t.GetComponent<CanvasRenderer>();
                    if (cr != null)
                    {
                        UnityEngine.Object.DestroyImmediate(cr, true);
                        removed++;
                        objectCount++;
                    }
                }
            }
            return removed;
        }

        // ---------- Dashboard sanity (non-breaking) ----------
        static int AutoWireDashboardIfPresent()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded || scene.name != "Dashboard") return 0;

            var ctrl = UnityEngine.Object.FindFirstObjectByType<GG.Game.DashboardSceneController>(FindObjectsInactive.Include);
            if (!ctrl) return 0;

            var so = new SerializedObject(ctrl);
            var t1 = so.FindProperty("titleLine1").objectReferenceValue as TMP_Text;
            var t2 = so.FindProperty("titleLine2").objectReferenceValue as TMP_Text;
            var lg = so.FindProperty("teamLogo").objectReferenceValue as Image;
            int wired = 0;

            var root = UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include)?.transform;
            if (root)
            {
                if (!t1)
                {
                    t1 = PickHeaderTMP(root, pickBiggest:true);
                    so.FindProperty("titleLine1").objectReferenceValue = t1;
                    wired += t1 ? 1 : 0;
                }
                if (!t2)
                {
                    t2 = PickHeaderTMP(root, pickBiggest:false, exclude:t1);
                    so.FindProperty("titleLine2").objectReferenceValue = t2;
                    wired += t2 ? 1 : 0;
                }
                if (!lg)
                {
                    lg = PickLogo(root);
                    so.FindProperty("teamLogo").objectReferenceValue = lg;
                    wired += lg ? 1 : 0;
                }
                if (wired > 0) so.ApplyModifiedPropertiesWithoutUndo();
            }

            if (wired > 0) EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[GGCleanup] Dashboard auto-wire: {wired} field(s) assigned.");
            return wired;
        }

        static TMP_Text PickHeaderTMP(Transform root, bool pickBiggest, TMP_Text exclude = null)
        {
            var tmps = root.GetComponentsInChildren<TMP_Text>(true);
            var topHalf = tmps.Where(t => ((RectTransform)t.transform).anchoredPosition.y > -200f && t != exclude);
            if (pickBiggest) return topHalf.OrderByDescending(t => t.fontSize).FirstOrDefault();
            return topHalf.OrderByDescending(t => t.fontSize).Skip(1).FirstOrDefault() ?? topHalf.FirstOrDefault();
        }

        static Image PickLogo(Transform root)
        {
            var imgs = root.GetComponentsInChildren<Image>(true);
            return imgs.OrderBy(i => Mathf.Abs(((RectTransform)i.transform).rect.width - ((RectTransform)i.transform).rect.height))
                       .FirstOrDefault();
        }

        // ---------- Unused assets report (no deletion) ----------
        static string ReportUnusedAssets()
        {
            // 1) Seeds = scenes in build + Assets/Resources + StreamingAssets + all prefabs (conservative)
            var seeds = new HashSet<string>();
            foreach (var s in EditorBuildSettings.scenes)
                if (s.enabled) seeds.Add(s.path);

            seeds.UnionWith(AssetDatabase.FindAssets("", new[] { "Assets/Resources", "Assets/StreamingAssets" })
                                         .Select(AssetDatabase.GUIDToAssetPath)
                                         .Where(p => !string.IsNullOrEmpty(p)));

            // 2) Expand dependencies
            var used = new HashSet<string>(seeds);
            foreach (var p in seeds.ToList())
            {
                foreach (var d in AssetDatabase.GetDependencies(p, true))
                    used.Add(d);
            }

            // 3) Candidates = non-code assets under Assets/ not in 'used'
            var all = AssetDatabase.FindAssets("", new[] { "Assets" })
                                   .Select(AssetDatabase.GUIDToAssetPath)
                                   .Where(p => !string.IsNullOrEmpty(p))
                                   .ToList();

            bool IsCode(string path) =>
                path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/Editor/");

            var candidates = all.Where(p =>
                                   !used.Contains(p) &&
                                   !IsCode(p) &&
                                   !Directory.Exists(p) &&
                                   !p.StartsWith("Assets/Addressables", StringComparison.OrdinalIgnoreCase)) // too risky to guess
                                 .OrderBy(p => p)
                                 .ToList();

            var outPath = "ProjectSettings/GG_Cleanup_Report.txt";
            var lines = new List<string>
            {
                "GG Quick Cleanup — Unused Asset Candidates (report only)",
                "NOTE: This list is conservative and may include false positives (e.g., Addressables, runtime loads).",
                "Do NOT delete blindly.",
                "",
                $"Generated: {DateTime.Now}",
                ""
            };
            lines.AddRange(candidates);
            File.WriteAllLines(outPath, lines);

            Debug.Log($"[GGCleanup] Unused asset candidates: {candidates.Count}. Report written to {outPath}");
            return outPath;
        }
    }
}
#endif
