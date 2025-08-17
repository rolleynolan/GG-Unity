using UnityEngine;

public static class GGLog
{
    public static void Log(string message) => Debug.Log(message);
    public static void Log(object message) => Debug.Log(message);

    public static void Info(string message) => Debug.Log(message);
    public static void Info(object message) => Debug.Log(message);

    public static void Warn(string message) => Debug.LogWarning(message);
    public static void Warn(object message) => Debug.LogWarning(message);

    public static void Error(string message) => Debug.LogError(message);
    public static void Error(object message) => Debug.LogError(message);
    public static void Error(string message, System.Exception ex) => Debug.LogError(ex == null ? message : message + "\n" + ex);
}
