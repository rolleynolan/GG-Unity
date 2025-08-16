using System.IO;
using UnityEngine;

/// <summary>
/// Shared path helpers for accessing application directories.
/// </summary>
public static class GGPaths
{
    /// <summary>
    /// Returns a path inside <see cref="Application.streamingAssetsPath"/>.
    /// </summary>
    public static string Streaming(string file)
    {
        return Path.Combine(Application.streamingAssetsPath, file);
    }

    /// <summary>
    /// Returns a path inside <see cref="Application.persistentDataPath"/>.
    /// </summary>
    public static string Save(string file)
    {
        return Path.Combine(Application.persistentDataPath, file);
    }
}
