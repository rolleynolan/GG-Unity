using System.IO;
using UnityEngine;

public static class GGPaths
{
    public static string Streaming(string file)
    {
        return Path.Combine(Application.streamingAssetsPath, file);
    }

    public static string Save(string file)
    {
        return Path.Combine(Application.persistentDataPath, file);
    }
}
