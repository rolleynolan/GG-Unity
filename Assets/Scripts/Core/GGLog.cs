using UnityEngine;

/// <summary>
/// Centralized logging helpers for the project.
/// Wraps Unity's <see cref="Debug"/> methods so callers use a single API.
/// </summary>
public static class GGLog
{
    public static void Log(string message)   => Debug.Log(message);
    public static void Info(string message)  => Debug.Log(message);
    public static void Warn(string message)  => Debug.LogWarning(message);
    public static void Error(string message) => Debug.LogError(message);

    public static void Log(object context)   => Debug.Log(context);
    public static void Info(object context)  => Debug.Log(context);
    public static void Warn(object context)  => Debug.LogWarning(context);
    public static void Error(object context) => Debug.LogError(context);
}
