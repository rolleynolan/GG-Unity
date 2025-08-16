using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace GG.Infra
{
    public static class GGLog
    {
        private static readonly object _lock = new object();
        private static string LogPath =>
            Path.Combine(Application.persistentDataPath, "gg_log.txt");

        public static void Info(string msg)  => Write("INFO", msg, null);
        public static void Warn(string msg)  => Write("WARN", msg, null);
        public static void Error(string msg, Exception ex = null) => Write("ERROR", msg, ex);

        private static void Write(string level, string msg, Exception ex)
        {
            var line = $"[{DateTime.UtcNow:O}] {level} {msg}{(ex != null ? $" :: {ex}" : "")}";
#if UNITY_EDITOR
            if (level == "ERROR") Debug.LogError(line);
            else if (level == "WARN") Debug.LogWarning(line);
            else Debug.Log(line);
#else
            // In builds, also echo to console
            if (level == "ERROR") Debug.LogError(line);
            else if (level == "WARN") Debug.LogWarning(line);
            else Debug.Log(line);
#endif
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(LogPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch { /* never crash on logging */ }
        }
    }
}
