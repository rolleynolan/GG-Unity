using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static GGPaths;
using static GGLog;

namespace GG.Bridge.Validation
{
    internal static class DataIO
    {
        [Serializable]
        private class ArrayWrapper<TItem>
        {
            public TItem[] items;
        }

        [Serializable]
        private class ListWrapper<TItem>
        {
            public List<TItem> items;
        }

        /// <summary>
        /// Loads JSON from an absolute path or a path relative to the project's data folder.
        /// </summary>
        public static T LoadJson<T>(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Warn("LoadJson called with empty path");
                return default;
            }

            var abs = Path.IsPathRooted(path) ? path : Path.Combine(DataRoot, path);

            if (!File.Exists(abs))
            {
                Warn($"JSON file not found: {abs}");
                return default;
            }

            try
            {
                var json = File.ReadAllText(abs);

                T obj;

                if (typeof(T).IsArray)
                {
                    var elemType = typeof(T).GetElementType();
                    var wrapperType = typeof(ArrayWrapper<>).MakeGenericType(elemType);
                    var wrapperJson = $"{{\"items\":{json}}}";
                    var wrapper = JsonUtility.FromJson(wrapperJson, wrapperType);
                    obj = (T)wrapperType.GetField("items").GetValue(wrapper);
                }
                else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                {
                    var elemType = typeof(T).GetGenericArguments()[0];
                    var wrapperType = typeof(ListWrapper<>).MakeGenericType(elemType);
                    var wrapperJson = $"{{\"items\":{json}}}";
                    var wrapper = JsonUtility.FromJson(wrapperJson, wrapperType);
                    obj = (T)wrapperType.GetField("items").GetValue(wrapper);
                }
                else
                {
                    obj = JsonUtility.FromJson<T>(json);
                }

                Info($"Loaded JSON: {abs}");
                return obj;
            }
            catch (Exception ex)
            {
                Warn($"Failed to load JSON: {abs} ({ex.Message})");
                return default;
            }
        }
    }
}
