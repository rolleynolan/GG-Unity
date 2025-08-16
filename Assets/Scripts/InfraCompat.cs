// Single compatibility shim so legacy `using GG.Infra;` works.
// Forwards to the global helpers defined without namespaces.
namespace GG.Infra
{
    public static class GGPaths
    {
        public static string ProjectRoot => global::GGPaths.ProjectRoot;
        public static string DataRoot() => global::GGPaths.DataRoot();
        public static string Data(string relative) => global::GGPaths.Data(relative);
        public static string ScheduleFile() => global::GGPaths.ScheduleFile();
        public static string ContractFile(string rel) => global::GGPaths.ContractFile(rel);
        public static string CapSheetFile(int year) => global::GGPaths.CapSheetFile(year);
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
