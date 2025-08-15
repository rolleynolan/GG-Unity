using System;

namespace GG.Season
{
    public static class SimpleSim
    {
        public static GameResult Sim(GameInfo g, Func<string,int> teamOvrLookup, int seed)
        {
            int homeOvr = teamOvrLookup(g.home) + 2;
            int awayOvr = teamOvrLookup(g.away);
            int diff = homeOvr - awayOvr;
            double winProb = 1.0 / (1.0 + Math.Exp(-diff / 6.0));
            var rng = new Random(seed);
            bool homeWins = rng.NextDouble() < winProb;
            int homeBase = 20 + diff / 2;
            int awayBase = 20 - diff / 2;
            int homeScore = Math.Max(0, homeBase + rng.Next(-7, 8));
            int awayScore = Math.Max(0, awayBase + rng.Next(-7, 8));
            if (homeWins && homeScore <= awayScore) homeScore = awayScore + rng.Next(1,4);
            if (!homeWins && awayScore <= homeScore) awayScore = homeScore + rng.Next(1,4);
            return new GameResult { week = g.week, home = g.home, away = g.away, homeScore = homeScore, awayScore = awayScore };
        }
    }
}
