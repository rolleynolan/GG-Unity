// Single compatibility shim so legacy `using GG.Infra;` works.
// Forwards to the global helpers defined without namespaces.
namespace GG.Infra
{
    public static class GGPaths
    {
        public static string ProjectRoot => global::GGPaths.ProjectRoot;
        public static string DataRoot => global::GGPaths.DataRoot;
        public static string StreamingRoot => global::GGPaths.StreamingRoot;
        public static string Json(string fileName) => global::GGPaths.Json(fileName);
        public static string Config(string fileName) => global::GGPaths.Config(fileName);
        public static string Data(string relative) => global::GGPaths.Data(relative);
        public static string Streaming(string relative) => global::GGPaths.Streaming(relative);
        public static string Save(string relative) => global::GGPaths.Save(relative);
        public static string ScheduleFile() => global::GGPaths.ScheduleFile();
        public static string ContractFile(string rel) => global::GGPaths.ContractFile(rel);
        public static string CapSheetFile(int year) => global::GGPaths.CapSheetFile(year);
        public const string TeamsJson = global::GGPaths.TeamsJson;
        public const string RostersByTeamJson = global::GGPaths.RostersByTeamJson;
        public const string ScheduleJson = global::GGPaths.ScheduleJson;
    }

    public static class GGLog
    {
        public static void Info(string msg)  => global::GGLog.Info(msg);
        public static void Warn(string msg)  => global::GGLog.Warn(msg);
        public static void Error(string msg, System.Exception ex = null)
            => global::GGLog.Error(msg, ex);
    }

    public static class TeamProvider
    {
        public static System.Collections.Generic.List<string> GetAbbrs()
            => new global::TeamProvider().GetAllTeamAbbrs();
    }
}
