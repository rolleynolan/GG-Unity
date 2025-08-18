// Single compatibility shim so legacy `using GG.Infra;` works.
// Forwards to the global helpers defined without namespaces.
namespace GG.Infra
{
    public static class GGPaths
    {
        public static string DataRoot      => global::GGPaths.DataRoot;
        public static string StreamingRoot => global::GGPaths.StreamingRoot;
        public static string Json(string fileName)   => global::GGPaths.Json(fileName);
        public static string Config(string fileName) => global::GGPaths.Config(fileName);
        public static string TeamsJson         => global::GGPaths.TeamsJson;
        public static string RostersByTeamJson => global::GGPaths.RostersByTeamJson;
        public static string ScheduleJson      => global::GGPaths.ScheduleJson;
    }

    public static class GGLog
    {
        public static void Log(string m)   => global::GGLog.Log(m);
        public static void Info(string m)  => global::GGLog.Info(m);
        public static void Warn(string m)  => global::GGLog.Warn(m);
        public static void Error(string m) => global::GGLog.Error(m);
    }

    public static class TeamProvider
    {
        public static string SelectedAbbr
        {
            get => global::TeamProvider.SelectedAbbr;
            set => global::TeamProvider.SelectedAbbr = value;
        }

        public static string GetSelectedTeamAbbreviation() => global::TeamProvider.GetSelectedTeamAbbreviation();
        public static void   SetSelectedTeamAbbreviation(string abbr) => global::TeamProvider.SetSelectedTeamAbbreviation(abbr);
    }
}
