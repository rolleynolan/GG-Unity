using System;
using System.IO;
using Newtonsoft.Json;
using GG.Infra;

namespace GG.Bridge.Validation
{
    internal static class DataIO
    {
        /// <summary>
        /// Loads JSON either from:
        ///  - a path rooted at /data (e.g., "data/cap/capsheet_2026.json" or "/data/…"), or
        ///  - a relative path within /data (e.g., "cap/capsheet_2026.json"), or
        ///  - an absolute path.
        /// </summary>
        public static T LoadJson<T>(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Empty path");

            string abs;
            var norm = path.Replace("\\", "/");

            if (Path.IsPathRooted(path))
            {
                abs = path;
            }
            else if (norm.StartsWith("data/", StringComparison.OrdinalIgnoreCase) ||
                     norm.StartsWith("/data/", StringComparison.OrdinalIgnoreCase))
            {
                abs = Path.GetFullPath(Path.Combine(GGPaths.ProjectRoot, norm.TrimStart('/')));
            }
            else
            {
                abs = GGPaths.Data(norm);
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
