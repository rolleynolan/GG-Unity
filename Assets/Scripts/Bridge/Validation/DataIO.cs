using System;
using System.IO;
using Newtonsoft.Json;
using GG.Infra; // for GGLog

namespace GG.Bridge.Validation
{
    internal static class DataIO
    {
        private static string ProjectRoot =>
            Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));

        private static string EnsureDataRoot()
        {
            var p = Path.Combine(ProjectRoot, "data");
            if (!Directory.Exists(p)) Directory.CreateDirectory(p);
            return p;
        }

        /// <summary>
        /// Loads JSON from:
        ///  - absolute path, or
        ///  - "data/..." or "/data/..." under the project root, or
        ///  - relative path under the "/data" folder (e.g., "cap/capsheet_2026.json").
        /// </summary>
        public static T LoadJson<T>(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Empty path", nameof(path));

            string abs;
            var norm = path.Replace("\\", "/");
            var dataRoot = EnsureDataRoot();

            if (Path.IsPathRooted(path))
            {
                abs = path;
            }
            else if (norm.StartsWith("data/", StringComparison.OrdinalIgnoreCase) ||
                     norm.StartsWith("/data/", StringComparison.OrdinalIgnoreCase))
            {
                abs = Path.GetFullPath(Path.Combine(ProjectRoot, norm.TrimStart('/')));
            }
            else
            {
                abs = Path.GetFullPath(Path.Combine(dataRoot, norm));
            }

            var json = File.ReadAllText(abs);

            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            };

            var obj = JsonConvert.DeserializeObject<T>(json, settings);
            GGLog.Info($"Loaded JSON: {abs}");
            return obj;
        }
    }
}

