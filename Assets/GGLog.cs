// 8/19/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public static class GGLog
{
    public static void Warning(string message)
    {
        Debug.LogWarning(message);
    }

    public static void Info(string message)
    {
        Debug.Log(message);
    }

    public static void Error(string message)
    {
        Debug.LogError(message);
    }
}