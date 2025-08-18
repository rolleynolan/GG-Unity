using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace GridironGM.Data
{
    public static class JsonLoader
    {
        public static T LoadFromStreamingAssets<T>(string filename)
        {
            string path = Path.Combine(Application.streamingAssetsPath, filename);
            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogError($"File not found: {path}");
                    return default;
                }
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load {filename}: {ex.Message}");
                return default;
            }
        }
    }
}
