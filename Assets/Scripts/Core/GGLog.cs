using UnityEngine;
public static class GGLog {
  public static bool Verbose =
#if GG_VERBOSE_LOGS
    true;
#else
    false;
#endif
  public static void Info(string m){ if (Verbose) Debug.Log($"[GG] {m}"); }
  public static void Warn(string m)=> Debug.LogWarning($"[GG] {m}");
  public static void Error(string m)=> Debug.LogError($"[GG] {m}");
}
