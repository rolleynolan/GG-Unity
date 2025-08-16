using System;
using System.Collections.Generic;

namespace GG.Bridge.Repositories
{
    // Minimal, self-contained loader for team abbreviations via TeamProvider.
    public static class TeamDirectory
    {
        public static List<string> GetAbbrs()
        {
            try
            {
                var abbrs = new TeamProvider().GetAllTeamAbbrs();
                if (abbrs != null && abbrs.Count > 0)
                {
                    return abbrs;
                }
                GGLog.Warn("TeamDirectory: provider returned no abbreviations.");
            }
            catch (Exception ex)
            {
                GGLog.Warn($"TeamDirectory: provider failed ({ex.Message}).");
            }
            return new List<string> { "ATL", "PHI", "DAL", "NYG" };
        }
    }
}
