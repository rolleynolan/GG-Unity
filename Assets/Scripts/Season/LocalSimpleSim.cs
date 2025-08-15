using System;

namespace GG.Game
{
    public interface ISimEngine { GameResult Simulate(GameInfo g, int seed); }

    public sealed class LocalSimpleSim : ISimEngine
    {
        private readonly Func<string,int> _teamOverall;
        public LocalSimpleSim(Func<string,int> teamOverallLookup) => _teamOverall = teamOverallLookup;

        public GameResult Simulate(GameInfo g, int seed)
        {
            var rnd  = new Random(seed);
            int home = _teamOverall(g.home) + 2;  // home-field boost
            int away = _teamOverall(g.away);

            double pHome = 1.0 / (1.0 + Math.Exp(-(home - away) / 6.0));

            int baseH = 10 + rnd.Next(8, 21);
            int baseA = 10 + rnd.Next(8, 21);
            if (rnd.NextDouble() < pHome) baseH += rnd.Next(0, 4);
            else                           baseA += rnd.Next(0, 4);

            if (baseH == baseA) { if (rnd.NextDouble() < pHome) baseH++; else baseA++; }

            return new GameResult { week = g.week, home = g.home, away = g.away, homeScore = baseH, awayScore = baseA };
        }
    }
}
