using System.IO;
using UnityEngine;

public static class GGPaths
{
    public static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    public static string DataRoot
    {
        get
        {
            var path = Path.Combine(Application.persistentDataPath, "GGData");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }

    public static string StreamingRoot => Application.streamingAssetsPath;

    public static string Json(string fileName) => Path.Combine(DataRoot, fileName);
    public static string Config(string fileName) => Path.Combine(StreamingRoot, fileName);

    public const string TeamsJson = "teams.json";
    public const string RostersByTeamJson = "rosters_by_team.json";
    public const string ScheduleJson = "schedule.json";

    public static string Data(string relative)
    {
        var abs = Path.GetFullPath(Path.Combine(DataRoot, relative.TrimStart('/', '\\')));
        var dir = Path.GetDirectoryName(abs);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return abs;
    }

    public static string Streaming(string relative)
    {
        return Path.GetFullPath(Path.Combine(StreamingRoot, relative.TrimStart('/', '\\')));
    }

    public static string Save(string relative)
    {
        var abs = Path.GetFullPath(Path.Combine(Application.persistentDataPath, relative.TrimStart('/', '\\')));
        var dir = Path.GetDirectoryName(abs);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return abs;
    }

    public static string ScheduleFile() => Data("schedule.json");
    public static string ContractFile(string rel) => Data(Path.Combine("contracts", rel));
    public static string CapSheetFile(int year) => Data(Path.Combine("cap", $"capsheet_{year}.json"));
}
