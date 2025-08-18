#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(TabController))]
public class TabControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Auto-Bind Tabs (from Hierarchy)"))
        {
            AutoBind((TabController)target);
        }
    }

    private void AutoBind(TabController ctrl)
    {
        var so = new SerializedObject(ctrl);
        var tabsProp = so.FindProperty("tabs");

        // Find Tabs container (prefer a child named "Tabs")
        Transform tabsRoot = FindByName(ctrl.transform.root, "Tabs");
        if (tabsRoot == null)
            tabsRoot = Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(t => t.name == "Tabs");

        // Find Body container (for panels)
        Transform bodyRoot = FindByName(ctrl.transform.root, "Body");
        if (bodyRoot == null)
            bodyRoot = Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(t => t.name == "Body");

        if (tabsRoot == null || bodyRoot == null)
        {
            EditorUtility.DisplayDialog("Auto-Bind Tabs",
                "Couldn't locate 'Tabs' or 'Body' containers. Name them exactly 'Tabs' and 'Body'.",
                "OK");
            return;
        }

        // Collect immediate children with Toggle or Button
        var tabItems = tabsRoot.Cast<Transform>()
            .Select(t => new {
                go = t.gameObject,
                toggle = t.GetComponent<Toggle>(),
                button = t.GetComponent<Button>(),
                name = t.name
            })
            .Where(x => x.toggle != null || x.button != null)
            .ToList();

        if (tabItems.Count == 0)
        {
            EditorUtility.DisplayDialog("Auto-Bind Tabs", "No Toggles or Buttons found under 'Tabs'.", "OK");
            return;
        }

        // Ensure a ToggleGroup if using toggles
        var anyToggle = tabItems.Any(x => x.toggle != null);
        ToggleGroup group = null;
        if (anyToggle)
        {
            group = tabsRoot.GetComponent<ToggleGroup>();
            if (group == null) group = Undo.AddComponent<ToggleGroup>(tabsRoot.gameObject);
            group.allowSwitchOff = false;
            foreach (var x in tabItems.Where(x => x.toggle != null))
                x.toggle.group = group;
        }

        // Build serialized array
        tabsProp.arraySize = tabItems.Count;

        for (int i = 0; i < tabItems.Count; i++)
        {
            var x = tabItems[i];

            // Derive id: strip "Tab" prefix if present
            string id = x.name.StartsWith("Tab", StringComparison.OrdinalIgnoreCase)
                        ? x.name.Substring(3)
                        : x.name;

            // Find panel named "Panel<ID>"
            string expectedPanelName = "Panel" + id;
            var panelTf = FindByName(bodyRoot, expectedPanelName);
            if (panelTf == null)
            {
                // Try global search
                panelTf = Resources.FindObjectsOfTypeAll<Transform>()
                    .FirstOrDefault(t => t.name == expectedPanelName);
            }

            var elem = tabsProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("id").stringValue = id;
            elem.FindPropertyRelative("button").objectReferenceValue = x.button;
            elem.FindPropertyRelative("toggle").objectReferenceValue = x.toggle;
            elem.FindPropertyRelative("panel").objectReferenceValue = panelTf ? panelTf.gameObject : null;
            elem.FindPropertyRelative("firstSelected").objectReferenceValue = null; // optional
            // onShown is a UnityEvent; leave default
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(ctrl);

        var missingPanels = new List<string>();
        for (int i = 0; i < tabItems.Count; i++)
        {
            var id = tabsProp.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue;
            var panelRef = tabsProp.GetArrayElementAtIndex(i).FindPropertyRelative("panel").objectReferenceValue;
            if (panelRef == null) missingPanels.Add(id);
        }

        if (missingPanels.Count > 0)
        {
            EditorUtility.DisplayDialog("Auto-Bind Tabs",
                "Bound tabs, but missing panels for: " + string.Join(", ", missingPanels) +
                "\n(Create GameObjects named 'Panel<ID>' under 'Body'.)",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Auto-Bind Tabs", "Tabs bound successfully.", "Nice");
        }
    }

    private Transform FindByName(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            var f = FindByName(c, name);
            if (f != null) return f;
        }
        return null;
    }
}
#endif
