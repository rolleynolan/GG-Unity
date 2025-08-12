#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RosterPanelPrefabTools
{
    const string PrefabPath = "Assets/Prefabs/UI/RosterPanel.prefab";
    const string DashboardScenePath = "Assets/Scenes/Dashboard.unity";

    [MenuItem("GridironGM/Build/Make RosterPanel Prefab (from TeamSelection)")]
    public static void MakeRosterPanelPrefab()
    {
        var go = Object.FindFirstObjectByType<RosterPanelUI>(FindObjectsInactive.Include)?.gameObject;
        if (go == null) { Debug.LogError("RosterPanelUI not found in current scene. Open TeamSelection and try again."); return; }

        // Ensure folder
        System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");

        // Save/replace prefab
        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, PrefabPath, InteractionMode.UserAction);
        if (prefab == null) { Debug.LogError("Failed to create RosterPanel prefab."); return; }
        Debug.Log($"[RosterPrefab] Saved {PrefabPath}");
    }

    [MenuItem("GridironGM/Build/Inject RosterPanel into Dashboard")]
    public static void InjectIntoDashboard()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null) { Debug.LogError($"Prefab missing at {PrefabPath}. Run Make RosterPanel Prefab first."); return; }

        // Open Dashboard scene
        var scene = EditorSceneManager.OpenScene(DashboardScenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) { Debug.LogError($"Cannot open scene {DashboardScenePath}"); return; }

        // Find PanelRoster & DashboardController
        GameObject panelRoster = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            panelRoster = FindChildRecursive(root.transform, "PanelRoster")?.gameObject ?? panelRoster;
        }
        if (panelRoster == null) { Debug.LogError("PanelRoster not found in Dashboard scene."); return; }

        // Clear existing children under PanelRoster (optional)
        for (int i = panelRoster.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(panelRoster.transform.GetChild(i).gameObject);

        // Instantiate prefab under PanelRoster
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.name = "RosterPanel";
        instance.transform.SetParent(panelRoster.transform, false);
        Stretch(instance.GetComponent<RectTransform>());

        // Wire DashboardController.rosterPanel
        var dc = Object.FindFirstObjectByType<DashboardController>(FindObjectsInactive.Include);
        var rp = instance.GetComponent<RosterPanelUI>();
        if (dc == null || rp == null) { Debug.LogError("DashboardController or RosterPanelUI missing."); return; }

        var so = new SerializedObject(dc);
        so.FindProperty("rosterPanel").objectReferenceValue = rp;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[RosterPrefab] Injected into Dashboard and wired DashboardController.rosterPanel");
    }

    static RectTransform FindChildRecursive(Transform t, string name)
    {
        if (t.name == name) return t as RectTransform;
        for (int i = 0; i < t.childCount; i++)
        {
            var r = FindChildRecursive(t.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }

    static void Stretch(RectTransform rt)
    {
        if (!rt) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif
