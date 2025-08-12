#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class RosterPanelPrefabTools
{
    const string PrefabPath = "Assets/Prefabs/UI/RosterPanel.prefab";
    const string DashboardScenePath = "Assets/Scenes/Dashboard.unity";
    const string PanelRosterName = "PanelRoster";

    [MenuItem("GridironGM/Build/Make RosterPanel Prefab (from TeamSelection)")]
    public static void MakeRosterPanelPrefab()
    {
        // Find ANY MonoBehaviour whose type name is exactly "RosterPanelUI" (ignore namespace)
        var all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        MonoBehaviour rp = null;
        foreach (var mb in all)
        {
            if (mb != null && mb.GetType().Name == "RosterPanelUI") { rp = mb; break; }
        }

        if (rp == null)
        {
            EditorUtility.DisplayDialog("RosterPanel Prefab", "RosterPanelUI not found in the open scene. Open TeamSelection and try again.", "OK");
            Debug.LogError("RosterPanelUI not found in the open scene.");
            return;
        }

        Directory.CreateDirectory("Assets/Prefabs/UI");

        var go = rp.gameObject;
        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, PrefabPath, InteractionMode.UserAction);
        if (prefab == null) { Debug.LogError("Failed to create RosterPanel prefab."); return; }
        Debug.Log($"[RosterPrefab] Saved {PrefabPath}");
    }

    [MenuItem("GridironGM/Build/Inject RosterPanel into Dashboard")]
    public static void InjectIntoDashboard()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null) { Debug.LogError($"Prefab missing at {PrefabPath}. Run 'Make RosterPanel Prefab' first."); return; }

        var scene = EditorSceneManager.OpenScene(DashboardScenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) { Debug.LogError($"Cannot open scene {DashboardScenePath}"); return; }

        // Find PanelRoster
        GameObject panelRoster = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            var found = FindChildRecursive(root.transform, PanelRosterName);
            if (found != null) { panelRoster = found.gameObject; break; }
        }
        if (panelRoster == null) { Debug.LogError($"'{PanelRosterName}' not found in Dashboard scene."); return; }

        // Clear children
        for (int i = panelRoster.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(panelRoster.transform.GetChild(i).gameObject);

        // Instantiate prefab
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.name = "RosterPanel";
        instance.transform.SetParent(panelRoster.transform, false);
        Stretch(instance.GetComponent<RectTransform>());

        // Find DashboardController BY NAME (ignore namespace)
        MonoBehaviour dashboardController = null;
        var all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            if (mb != null && mb.GetType().Name == "DashboardController") { dashboardController = mb; break; }
        }
        if (dashboardController == null) { Debug.LogError("DashboardController not found in scene."); return; }

        // Find RosterPanelUI on the new instance
        MonoBehaviour rosterPanelUI = null;
        foreach (var mb in instance.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb.GetType().Name == "RosterPanelUI") { rosterPanelUI = mb; break; }
        }
        if (rosterPanelUI == null) { Debug.LogError("RosterPanelUI component not found on the prefab instance."); return; }

        // Assign serialized field named 'rosterPanel' on DashboardController
        var so = new SerializedObject(dashboardController);
        var prop = so.FindProperty("rosterPanel");
        if (prop == null)
        {
            Debug.LogError("DashboardController is missing a serialized field named 'rosterPanel'.");
            return;
        }
        prop.objectReferenceValue = rosterPanelUI;
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

