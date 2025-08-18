using UnityEngine;

public static class GGLog
{
    public static void Log(string m)   => Debug.Log(m);
    public static void Info(string m)  => Debug.Log(m);
    public static void Warn(string m)  => Debug.LogWarning(m);
    public static void Error(string m) => Debug.LogError(m);

    public static void Log(object o)   => Debug.Log(o);
    public static void Info(object o)  => Debug.Log(o);
    public static void Warn(object o)  => Debug.LogWarning(o);
    public static void Error(object o) => Debug.LogError(o);
}
