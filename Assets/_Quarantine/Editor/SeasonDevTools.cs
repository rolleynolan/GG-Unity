#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class SeasonDevTools
{
    [MenuItem("GridironGM/Dev/Clear Season Save")]
    public static void Clear()
    {
        var p = Path.Combine(Application.persistentDataPath, "season.json");
        if (File.Exists(p)) File.Delete(p);
        Debug.Log("[Dev] Deleted season save.");
    }
}
#endif
