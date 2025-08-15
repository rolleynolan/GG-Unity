#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GG.Game;

public static class DashboardFixer
{
    [MenuItem("GridironGM/Dashboard/Install Simple Binder (Auto-Wire)")]
    public static void Install()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Dashboard", "Open the Dashboard scene first.", "OK");
            return;
        }

        var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (!canvas)
        {
            EditorUtility.DisplayDialog("Dashboard", "No Canvas found in this scene.", "OK");
            return;
        }

        // Remove older patches so nothing conflicts
        RemoveIfExists("DashboardBootstrap");
        RemoveIfExists("DashboardHeaderBinder");
        RemoveIfExists("DashboardTeamEnforcer");

        // Add or reuse controller
        var ctrl = UnityEngine.Object.FindFirstObjectByType<DashboardSceneController>(FindObjectsInactive.Include);
        if (!ctrl)
        {
            var go = new GameObject("DashboardSceneController");
            go.transform.SetParent(canvas.transform, false);
            ctrl = go.AddComponent<DashboardSceneController>();
        }

        // Try to auto-wire title/Logo from existing UI
        AutoWire(ctrl, canvas.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[DashboardFixer] Simple binder installed and auto-wired.");
    }

    [MenuItem("GridironGM/Dashboard/Remove Old Patch Components")]
    public static void RemoveOldPatches()
    {
        int n = 0;
        n += RemoveIfExists("DashboardBootstrap");
        n += RemoveIfExists("DashboardHeaderBinder");
        n += RemoveIfExists("DashboardTeamEnforcer");
        Debug.Log($"[DashboardFixer] Removed {n} old patch component(s).");
    }

    static int RemoveIfExists(string typeName)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(a => a.GetTypes())
                     .FirstOrDefault(t => t.Name == typeName);
        if (type == null) return 0;

        var comps = UnityEngine.Object.FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in comps) UnityEngine.Object.DestroyImmediate((UnityEngine.Object)c, true);
        return comps.Length;
    }

    static void AutoWire(DashboardSceneController ctrl, Transform root)
    {
        // Heuristics: pick two biggest TMPs near top for the header, and a square-ish Image as logo.
        var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        var topHalf = tmps.Where(t =>
        {
            var y = ((RectTransform)t.transform).anchoredPosition.y;
            return y > -200f; // roughly near top bar
        }).ToArray();

        var title1 = topHalf.OrderByDescending(t => t.fontSize).FirstOrDefault();
        var title2 = topHalf.Where(t => t != title1).OrderByDescending(t => t.fontSize).FirstOrDefault();

        var imgs = root.GetComponentsInChildren<Image>(true);
        var logo = imgs.OrderBy(i => Mathf.Abs(((RectTransform)i.transform).rect.width - ((RectTransform)i.transform).rect.height))
                       .FirstOrDefault();

        var so = new SerializedObject(ctrl);
        so.FindProperty("titleLine1").objectReferenceValue = title1;
        so.FindProperty("titleLine2").objectReferenceValue = title2;
        so.FindProperty("teamLogo").objectReferenceValue   = logo;
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"[DashboardFixer] Auto-wired: title1='{title1?.name}', title2='{title2?.name}', logo='{logo?.name}'.");
    }
}
#endif
