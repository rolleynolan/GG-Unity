#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class TeamLogoDatabaseBuilder
{
    [MenuItem("GridironGM/Logos/Rebuild Logo Database (from Assets/teamsprites)")]
    public static void Rebuild()
    {
        var db = ScriptableObject.CreateInstance<TeamLogoDatabase>();
        string folder = "Assets/teamsprites"; // your logos live here
        var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (!sprite) continue;
            var abbr = Path.GetFileNameWithoutExtension(path).ToUpperInvariant(); // e.g., KC.png -> KC
            db.entries.Add(new TeamLogoDatabase.Entry { abbreviation = abbr, sprite = sprite });
        }
        // Ensure Resources/Data exists and save there so runtime can load it
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data")) AssetDatabase.CreateFolder("Assets/Resources", "Data");
        var assetPath = "Assets/Resources/Data/TeamLogoDatabase.asset";
        AssetDatabase.CreateAsset(db, assetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Logos] Built TeamLogoDatabase with {db.entries.Count} sprites → {assetPath}");
    }
}
#endif

