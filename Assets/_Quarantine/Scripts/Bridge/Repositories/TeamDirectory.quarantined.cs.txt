using System.Collections.Generic;

namespace GG.Bridge.Repositories
{
    public static class TeamDirectory
    {
        public static List<string> GetAbbrs()
        {
            return new List<string> { "ATL", "PHI", "DAL", "NYG" };
        }
    }
}
