using System.IO;
using UnityEngine;
public static class GGPaths {
    public static string DataRoot => Path.Combine(Application.persistentDataPath,"GGData");
    public static string StreamingRoot => Application.streamingAssetsPath;
    public static string Json(string file)=>EnsureDirAndCombine(DataRoot,file);
    public static string Config(string file)=>Path.Combine(StreamingRoot,file);
    public static string TeamsJson => Path.Combine(StreamingRoot,"config/teams.json");
    public static string RostersByTeamJson => Json("rosters_by_team.json");
    public static string ScheduleJson => Json("schedule.json");
    static string EnsureDirAndCombine(string dir,string file){ if(!Directory.Exists(dir)) Directory.CreateDirectory(dir); return Path.Combine(dir,file); }
}
