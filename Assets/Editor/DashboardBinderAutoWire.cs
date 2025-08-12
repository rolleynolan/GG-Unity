#if UNITY_EDITOR
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DashboardBinderAutoWire
{
    [MenuItem("Tools/Gridiron GM/Auto-Wire Dashboard Binders")]
    public static void AutoWire()
    {
        var scene = SceneManager.GetActiveScene();
        var sb = new StringBuilder();
        if (!scene.IsValid())
        {
            Debug.LogError("No active scene.");
            return;
        }

        var roots = scene.GetRootGameObjects();
        var panelRoster = roots.FirstOrDefault(r => r.name == "PanelRoster");
        var panelSchedule = roots.FirstOrDefault(r => r.name == "PanelSchedule");
        var panelDepth = roots.FirstOrDefault(r => r.name == "PanelDepth");

        var playerRowPrefab = FindOrCreatePlayerRowUIPrefab(sb);
        var textRowPrefab = FindOrCreateTextRowPrefab(sb);

        if (panelRoster != null)
        {
            var binder = panelRoster.GetComponent<RosterBinder>() ?? Undo.AddComponent<RosterBinder>(panelRoster);
            Undo.RecordObject(binder, "Wire RosterBinder");
            var so = new SerializedObject(binder);
            so.FindProperty("contentParent").objectReferenceValue = FindContent(panelRoster.transform);
            so.FindProperty("playerRowPrefab").objectReferenceValue = playerRowPrefab;
            so.FindProperty("headerText").objectReferenceValue = FindHeader(panelRoster.transform);
            so.ApplyModifiedProperties();
            sb.AppendLine("Wired RosterBinder.");
        }
        else sb.AppendLine("PanelRoster not found.");

        if (panelSchedule != null)
        {
            var binder = panelSchedule.GetComponent<ScheduleBinder>() ?? Undo.AddComponent<ScheduleBinder>(panelSchedule);
            Undo.RecordObject(binder, "Wire ScheduleBinder");
            var so = new SerializedObject(binder);
            so.FindProperty("contentParent").objectReferenceValue = FindContent(panelSchedule.transform);
            so.FindProperty("textRowPrefab").objectReferenceValue = textRowPrefab;
            so.FindProperty("headerText").objectReferenceValue = FindHeader(panelSchedule.transform);
            so.ApplyModifiedProperties();
            sb.AppendLine("Wired ScheduleBinder.");
        }
        else sb.AppendLine("PanelSchedule not found.");

        if (panelDepth != null)
        {
            var binder = panelDepth.GetComponent<DepthChartsBinder>() ?? Undo.AddComponent<DepthChartsBinder>(panelDepth);
            Undo.RecordObject(binder, "Wire DepthChartsBinder");
            var so = new SerializedObject(binder);
            so.FindProperty("contentParent").objectReferenceValue = FindContent(panelDepth.transform);
            so.FindProperty("textRowPrefab").objectReferenceValue = textRowPrefab;
            so.FindProperty("headerText").objectReferenceValue = FindHeader(panelDepth.transform);
            so.FindProperty("slotsPerPosition").intValue = 2;
            so.ApplyModifiedProperties();
            sb.AppendLine("Wired DepthChartsBinder.");
        }
        else sb.AppendLine("PanelDepth not found.");

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        Debug.Log(sb.ToString());
    }

    static Transform FindContent(Transform panel)
    {
        return panel ? panel.Find("Scroll View/Viewport/Content") : null;
    }

    static TMP_Text FindHeader(Transform panel)
    {
        if (!panel) return null;
        foreach (Transform child in panel)
        {
            var txt = child.GetComponent<TMP_Text>();
            if (txt) return txt;
        }
        return null;
    }

    static GameObject FindOrCreatePlayerRowUIPrefab(StringBuilder sb)
    {
        var guids = AssetDatabase.FindAssets("t:Prefab PlayerRowUI");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            sb.AppendLine("Found PlayerRowUI prefab: " + path);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        var go = new GameObject("PlayerRowUI");
        go.AddComponent<HorizontalLayoutGroup>();
        var nameText = CreateTMP("NameText", go.transform);
        var posText = CreateTMP("PosText", go.transform);
        var ovrText = CreateTMP("OvrText", go.transform);
        var pru = go.AddComponent<PlayerRowUI>();
        pru.nameText = nameText;
        pru.posText = posText;
        pru.ovrText = ovrText;

        EnsurePrefabsFolder();
        var pathSave = "Assets/Prefabs/PlayerRowUI.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, pathSave);
        Object.DestroyImmediate(go);
        sb.AppendLine("Created PlayerRowUI prefab: " + pathSave);
        return prefab;
    }

    static GameObject FindOrCreateTextRowPrefab(StringBuilder sb)
    {
        var guids = AssetDatabase.FindAssets("t:Prefab TextRow");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            sb.AppendLine("Found TextRow prefab: " + path);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        var go = new GameObject("TextRow");
        go.AddComponent<LayoutElement>();
        var childText = CreateTMP("RowText", go.transform);

        EnsurePrefabsFolder();
        var pathSave = "Assets/Prefabs/TextRow.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, pathSave);
        Object.DestroyImmediate(go);
        sb.AppendLine("Created TextRow prefab: " + pathSave);
        return prefab;
    }

    static TMP_Text CreateTMP(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<TMP_Text>();
    }

    static void EnsurePrefabsFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
    }
}
#endif
