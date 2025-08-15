using System;
using System.Collections.Generic;

namespace GG.Game
{
    public static class ScheduleService
    {
        // Simple weekly pairings for a vertical slice.
        public static List<WeekGames> Generate(List<string> abbrs, int weeks = 14, int seed = 12345)
        {
            var rnd = new Random(seed);
            var weeksList = new List<WeekGames>(weeks);

            for (int w = 1; w <= weeks; w++)
            {
                var wg = new WeekGames { week = w };
                var list = new List<string>(abbrs);

                // Shuffle
                for (int i = list.Count - 1; i > 0; i--)
                { int j = rnd.Next(i + 1); (list[i], list[j]) = (list[j], list[i]); }

                // Pair neighbors
                for (int i = 0; i + 1 < list.Count; i += 2)
                {
                    var a = list[i]; var b = list[i + 1];
                    var home = rnd.NextDouble() < 0.5 ? a : b;
                    var away = home == a ? b : a;
                    wg.games.Add(new GameInfo { week = w, home = home, away = away });
                }
                weeksList.Add(wg);
            }
            return weeksList;
        }
    }
}
