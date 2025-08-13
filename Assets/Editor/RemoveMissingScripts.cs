#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class RemoveMissingScripts
{
    [MenuItem("GridironGM/Remove Missing Scripts In Scene")]
    public static void Remove()
    {
        int total = 0;
        foreach (var go in Object.FindObjectsOfType<GameObject>())
            total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        Debug.Log($"[Tools] Removed {total} missing script component(s) from scene.");
    }
}
#endif
