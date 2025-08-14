#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class PlayerRowPrefabBaker
{
    [MenuItem("GridironGM/UI/Bake PlayerRowUI Prefab")]
    public static void Bake()   // ← void, not Button
    {
        var guids = AssetDatabase.FindAssets("t:Prefab PlayerRowUI");
        if (guids.Length == 0)
        {
            Debug.LogWarning("[Bake] PlayerRowUI prefab not found.");
            return;
        }

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var root = PrefabUtility.LoadPrefabContents(path);

            // Ensure background image
            var img = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = new Color(0.10f, 0.18f, 0.28f, 1f); // matches list base

            // Ensure clickable (RosterPanelUI uses this for selection highlight)
            var btn = root.GetComponent<Button>() ?? root.AddComponent<Button>();  // ← assign it
            btn.transition = Selectable.Transition.None;

            // Ensure the four TMP children exist and sane text flags (no wrap, ellipsis)
            SetTMPFlags(root.transform, "NameText");
            SetTMPFlags(root.transform, "PosText");
            SetTMPFlags(root.transform, "OvrText");
            SetTMPFlags(root.transform, "AgeText");

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log($"[Bake] Hardened {path}");
        }
    }

    static void SetTMPFlags(Transform root, string name)
    {
        var t = root.Find(name);
        if (!t) return;
        var tmp = t.GetComponent<TMP_Text>();
        if (!tmp) return;
        tmp.enableAutoSizing  = false;
        tmp.textWrappingMode  = TextWrappingModes.NoWrap; // TMP 4.x
        tmp.overflowMode      = TextOverflowModes.Ellipsis;
    }
}
#endif
