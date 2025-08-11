#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using GridironGM.Data;

public static class BuildTeamLogoDatabase
{
    [MenuItem("GridironGM/Build Team Logo DB")]
    public static void BuildDB()
    {
        const string inputFolder = "Assets/teamsprites";
        const string resourcesFolder = "Assets/Resources";
        const string assetPath = "Assets/Resources/TeamLogoDB.asset";

        if (!Directory.Exists(resourcesFolder))
            Directory.CreateDirectory(resourcesFolder);

        var guids = AssetDatabase.FindAssets("t:Sprite", new[] { inputFolder });
        var list = new List<TeamLogoEntry>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;

            var file = Path.GetFileNameWithoutExtension(path); // e.g., ATL
            if (string.IsNullOrEmpty(file)) continue;

            list.Add(new TeamLogoEntry { abbreviation = file, sprite = sprite });
        }

        TeamLogoDatabase db = AssetDatabase.LoadAssetAtPath<TeamLogoDatabase>(assetPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<TeamLogoDatabase>();
            AssetDatabase.CreateAsset(db, assetPath);
        }

        db.SetEntries(list);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LogoDB] Built with {list.Count} entries -> {assetPath}");
    }
}
#endif
