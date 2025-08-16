using System.IO;
using UnityEngine;

public static class GGPaths
{
    public static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    public static string DataRoot()
    {
        var p = Path.Combine(ProjectRoot, "data");
        if (!Directory.Exists(p)) Directory.CreateDirectory(p);
        return p;
    }

    public static string Data(string relative)
    {
        var abs = Path.GetFullPath(Path.Combine(DataRoot(), relative.TrimStart('/', '\\')));
        var dir = Path.GetDirectoryName(abs);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return abs;
    }

    public static string Streaming(string relative)
    {
        return Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, relative.TrimStart('/', '\\')));
    }

    public static string Save(string relative)
    {
        var abs = Path.GetFullPath(Path.Combine(Application.persistentDataPath, relative.TrimStart('/', '\\')));
        var dir = Path.GetDirectoryName(abs);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return abs;
    }

    public static string ScheduleFile()           => Data("schedule.json");
    public static string ContractFile(string rel) => Data(Path.Combine("contracts", rel));
    public static string CapSheetFile(int year)   => Data(Path.Combine("cap", $"capsheet_{year}.json"));
}
