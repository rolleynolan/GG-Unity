#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class DisableWatermarkInScene
{
    [MenuItem("GridironGM/UI/Disable Watermarks In Open Scenes")]
    public static void Run()
    {
        int disabled = 0, removed = 0;

        foreach (var img in Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!img) continue;
            var n = img.name.ToLowerInvariant();
            var looksLikeWatermark =
                   n.Contains("watermark")
                || (img.preserveAspect && !img.raycastTarget && img.color.a <= 0.1f);

            if (!looksLikeWatermark) continue;

            var comps = img.GetComponents<Component>();
            if (comps.Length <= 2) // Transform + Image only
            {
                Object.DestroyImmediate(img.gameObject, true);
                removed++;
            }
            else
            {
                img.enabled = false;
                disabled++;
            }
        }

        Debug.Log($"[DisableWatermarks] Disabled {disabled}, removed {removed} watermark image(s) in open scenes.");
    }
}
#endif
