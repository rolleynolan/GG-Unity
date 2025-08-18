#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class LogoDatabaseBuilder
{
    [MenuItem("GridironGM/Logos/Rebuild Logo Database")]
    public static void Rebuild()
    {
        string[] sources = { "Assets/teamsprites", "Assets/Resources/TeamSprites", "Assets/TeamSprites" };
        const string resourcesDir = "Assets/Resources/Data";
        const string assetPath = resourcesDir + "/TeamLogoDatabase.asset";

        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(resourcesDir)) AssetDatabase.CreateFolder("Assets/Resources", "Data");

        var guids = sources
            .Where(AssetDatabase.IsValidFolder)
            .SelectMany(src => AssetDatabase.FindAssets("t:Sprite", new[] { src }))
            .Distinct()
            .ToArray();

        var db = ScriptableObject.CreateInstance<TeamLogoDatabase>();
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (!sprite) continue;
            var abbr = Path.GetFileNameWithoutExtension(path).Trim().ToUpperInvariant(); // KC.png -> KC
            db.items.Add(new TeamLogoDatabase.Entry { abbr = abbr, sprite = sprite });
        }

        var existing = AssetDatabase.LoadAssetAtPath<TeamLogoDatabase>(assetPath);
        if (!existing) AssetDatabase.CreateAsset(db, assetPath);
        else { existing.items = db.items; EditorUtility.SetDirty(existing); Object.DestroyImmediate(db, true); }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Logos] Built TeamLogoDatabase with {db.items.Count} sprites → {assetPath}");
    }
}
#endif
