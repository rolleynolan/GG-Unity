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
        bool IsTabLabel(TMP_Text t)
        {
            if (!t) return false;
            string n = t.name.ToLowerInvariant();
            string txt = (t.text ?? "").Trim().ToLowerInvariant();
            if (t.GetComponentInParent<UnityEngine.UI.Selectable>() != null) return true; // under a button/tab
            return
                n.Contains("tab") || n.Contains("button") ||
                n.Contains("roster") || n.Contains("depth") || n.Contains("schedule") ||
                txt == "roster" || txt == "depth charts" || txt == "team schedule";
        }

        // Top-half TMPs, biggest first, skipping tab labels.
        var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true)
                       .Where(t => ((RectTransform)t.transform).anchoredPosition.y > -200f)
                       .Where(t => !IsTabLabel(t))
                       .OrderByDescending(t => t.fontSize)
                       .ToList();

        var title1 = tmps.FirstOrDefault();
        var title2 = tmps.Skip(1).FirstOrDefault();

        bool LooksLikeBadLogo(UnityEngine.UI.Image img)
        {
            if (!img) return true;
            string n = img.name.ToLowerInvariant();
            if (n.Contains("viewport") || n.Contains("mask") || n.Contains("content") ||
                n.Contains("scroll") || n.Contains("background") || n.Contains("panel"))
                return true;
            var rt = (RectTransform)img.transform;
            var r = rt.rect;
            // prefer square-ish, not microscopic/huge
            float squareness = Mathf.Abs(r.width - r.height);
            if (r.width < 16f || r.height < 16f) return true;
            return false;
        }

        // Prefer objects named logo/crest; otherwise nearest square-ish Image to title1.
        var imgs = root.GetComponentsInChildren<UnityEngine.UI.Image>(true).ToList();
        var namedLogo = imgs.FirstOrDefault(i =>
            i.name.ToLowerInvariant().Contains("logo") || i.name.ToLowerInvariant().Contains("crest"));
        UnityEngine.UI.Image logo = null;

        if (namedLogo && !LooksLikeBadLogo(namedLogo)) logo = namedLogo;
        else if (title1)
        {
            logo = imgs.Where(i => !LooksLikeBadLogo(i))
                       .OrderBy(i =>
                       {
                           var rt = (RectTransform)i.transform;
                           var r = rt.rect;
                           float square = Mathf.Abs(r.width - r.height);
                           float d = Vector3.Distance(i.transform.position, title1.transform.position);
                           return square * 0.6f + d * 0.4f;
                       })
                       .FirstOrDefault();
        }

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
