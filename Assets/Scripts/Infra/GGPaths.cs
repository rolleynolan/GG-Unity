using System.IO;
using UnityEngine;

namespace GG.Infra
{
    /// <summary>Project path helpers. Treats /data as a sibling of Assets/.</summary>
    public static class GGPaths
    {
        public static string ProjectRoot =>
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        public static string DataRoot()
        {
            var p = Path.Combine(ProjectRoot, "data");
            if (!Directory.Exists(p)) Directory.CreateDirectory(p);
            return p;
        }

        /// <summary>Returns an absolute path inside /data, creating parent dirs if needed.</summary>
        public static string Data(string relative)
        {
            var abs = Path.GetFullPath(Path.Combine(DataRoot(), relative.TrimStart('/', '\\')));
            var dir = Path.GetDirectoryName(abs);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return abs;
        }

        public static string CapSheetFile(int year) => Data(Path.Combine("cap", $"capsheet_{year}.json"));
    }
}
