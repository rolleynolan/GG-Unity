#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class BuildTeamLogoDatabase
{
    [MenuItem("GridironGM/Build Team Logo DB")]
    public static void BuildDB()
    {
        var inputFolders = new[] { "Assets/TeamSprites", "Assets/teamsprites" }; // handle either casing
        const string resourcesPath = "Assets/Resources";
        const string assetPath     = "Assets/Resources/TeamLogoDB.asset";

        if (!Directory.Exists(resourcesPath))
            Directory.CreateDirectory(resourcesPath);

        var spriteGuids = AssetDatabase.FindAssets("t:Sprite", inputFolders);
        var list = new List<TeamLogoEntry>();

        string Norm(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Trim().Replace("_", "").Replace("-", "").Replace(" ", "");
            return s.ToUpperInvariant();
        }

        // Pull an abbreviation from filename (ATL.png, atl_logo.png, ATL-Logo@2x.png, etc.)
        var re = new Regex(@"([A-Za-z]{2,4})", RegexOptions.Compiled);

        foreach (var guid in spriteGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;

            var file = Path.GetFileNameWithoutExtension(path);
            var m = re.Match(file);
            if (!m.Success) continue;

            var abbr = Norm(m.Groups[1].Value);
            if (string.IsNullOrEmpty(abbr)) continue;

            list.Add(new TeamLogoEntry { abbreviation = abbr, sprite = sprite });
        }

        var db = AssetDatabase.LoadAssetAtPath<TeamLogoDatabase>(assetPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<TeamLogoDatabase>();
            AssetDatabase.CreateAsset(db, assetPath);
        }

        db.SetEntries(list);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LogoDB] Built with {list.Count} entries from {string.Join(", ", inputFolders)} -> {assetPath}");
    }
}
#endif
