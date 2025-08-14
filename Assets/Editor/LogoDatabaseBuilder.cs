#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class LogoDatabaseBuilder
{
    [MenuItem("GridironGM/Logos/Rebuild Logo Database (from Assets/teamsprites)")]
    public static void Rebuild()
    {
        const string srcFolder = "Assets/teamsprites";
        const string resourcesFolder = "Assets/Resources/Data";
        const string assetPath = resourcesFolder + "/TeamLogoDatabase.asset";

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(resourcesFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "Data");

        var guids = AssetDatabase.FindAssets("t:Sprite", new[] { srcFolder });
        var sprites = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(s => s != null)
            .ToList();

        var db = ScriptableObject.CreateInstance<TeamLogoDatabase>();
        foreach (var s in sprites)
        {
            // Map by filename (without extension); trim whitespace, uppercase.
            var file = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(s));
            var abbr = file.Trim().ToUpperInvariant(); // Expect KC.png, WAS.png, etc.
            db.items.Add(new TeamLogoDatabase.Entry { abbr = abbr, sprite = s });
        }

        var existing = AssetDatabase.LoadAssetAtPath<TeamLogoDatabase>(assetPath);
        if (existing == null) AssetDatabase.CreateAsset(db, assetPath);
        else { existing.items = db.items; EditorUtility.SetDirty(existing); }
        AssetDatabase.SaveAssets();
        Debug.Log($"[LogoDatabaseBuilder] Rebuilt logo DB with {db.items.Count} entries at {assetPath}");
    }
}
#endif
