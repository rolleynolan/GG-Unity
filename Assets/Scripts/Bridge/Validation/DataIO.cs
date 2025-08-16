using System.IO;
using Newtonsoft.Json;
using GG.Infra;

namespace GG.Bridge.Validation
{
    internal static class DataIO
    {
        public static T LoadJson<T>(string relativePath)
        {
            var abs = GGPaths.Project(relativePath);
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
