using System.IO;
using UnityEngine;

public static class GGPaths
{
    /// <summary>
    /// Root folder for persistent GG data.
    /// </summary>
    public static string DataRoot => SaveRoot;

    /// <summary>
    /// Root folder for persistent GG data.
    /// </summary>
    public static string SaveRoot => Path.Combine(Application.persistentDataPath, "GGData");

    public static string CombineSave(string file) => Path.Combine(SaveRoot, file);

    /// <summary>
    /// Root folder for streaming assets.
    /// </summary>
    public static string StreamingRoot => Application.streamingAssetsPath;

    /// <summary>
    /// Combine a file name with the persistent data root and ensure the directory exists.
    /// </summary>
    public static string Save(string file) => EnsureDirAndCombine(SaveRoot, file);

    /// <summary>
    /// Combine a file name with the streaming assets root.
    /// </summary>
    public static string Streaming(string file) => Path.Combine(StreamingRoot, file);

    // Legacy helpers retained for backward compatibility
    public static string Json(string file) => Save(file);
    public static string Config(string file) => Streaming(file);
    


    public static string TeamsJson => Streaming("config/teams.json");
    public static string RostersByTeamJson => Save("rosters_by_team.json");
    public static string ScheduleJson => Save("schedule.json");

    static string EnsureDirAndCombine(string dir, string file)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return Path.Combine(dir, file);
    }
}
