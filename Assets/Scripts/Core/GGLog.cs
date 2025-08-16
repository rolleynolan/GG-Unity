using UnityEngine;

public static class GGLog
{
    public static void Info(string msg)  => Debug.Log(msg);
    public static void Warn(string msg)  => Debug.LogWarning(msg);
    public static void Error(string msg, System.Exception ex = null)
        => Debug.LogError(ex == null ? msg : $"{msg}\n{ex}");
}
