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

        // ✅ Guard: only install in the Dashboard scene
        if (!scene.isLoaded || scene.name != "Dashboard")
        {
            EditorUtility.DisplayDialog(
                "Dashboard Binder",
                "Open the 'Dashboard' scene before running this installer.\n\n" +
                $"Current scene: {(scene.isLoaded ? scene.name : "(none)")}",
                "OK");
            return;
        }

        var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (!canvas)
        {
            EditorUtility.DisplayDialog("Dashboard", "No Canvas found in this scene.", "OK");
            return;
        }

        RemoveIfExists("DashboardBootstrap");
        RemoveIfExists("DashboardHeaderBinder");
        RemoveIfExists("DashboardTeamEnforcer");

        var ctrl = UnityEngine.Object.FindFirstObjectByType<DashboardSceneController>(FindObjectsInactive.Include);
        if (!ctrl)
        {
            var go = new GameObject("DashboardSceneController");
            go.transform.SetParent(canvas.transform, false);
            ctrl = go.AddComponent<DashboardSceneController>();
        }

        AutoWire(ctrl, canvas.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[DashboardFixer] Simple binder installed and auto-wired.");
    }

    [MenuItem("GridironGM/Dashboard/Remove Binder From Non-Dashboard Scenes")]
    public static void RemoveBinderFromWrongScenes()
    {
        int removed = 0;
        foreach (var ctrl in UnityEngine.Object.FindObjectsByType<DashboardSceneController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var s = ctrl.gameObject.scene;
            if (s.IsValid() && s.isLoaded && s.name != "Dashboard")
            {
                UnityEngine.Object.DestroyImmediate(ctrl.gameObject, true);
                removed++;
            }
        }
        Debug.Log($"[DashboardFixer] Removed {removed} DashboardSceneController object(s) from non-Dashboard scenes.");
    }

    static void AutoWire(DashboardSceneController ctrl, Transform root)
    {
        // Heuristics: two biggest TMPs near the top; and a square-ish Image as the logo.
        var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        var topHalf = tmps.Where(t => ((RectTransform)t.transform).anchoredPosition.y > -200f).ToArray();
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
}
#endif
