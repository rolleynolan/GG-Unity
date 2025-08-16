using System.IO;
using UnityEngine;

public static class GGLog {
  public static bool Verbose =
#if GG_VERBOSE_LOGS
    true;
#else
    false;
#endif

  static string logFile;
  static string LogFile { get { if (logFile==null) logFile = GGPaths.Save("gg_log.txt"); return logFile; } }

  static void Write(string level, string msg){
#if !UNITY_EDITOR
    try { File.AppendAllText(LogFile, $"[{level}] {msg}\n"); } catch { }
#endif
  }

  public static void Info(string m){ if (Verbose) Debug.Log($"[GG] {m}"); Write("I", m); }
  public static void Warn(string m){ Debug.LogWarning($"[GG] {m}"); Write("W", m); }
  public static void Error(string m){ Debug.LogError($"[GG] {m}"); Write("E", m); }
}
