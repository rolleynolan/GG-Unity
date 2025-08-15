using System;
using System.Collections.Generic;
using System.Linq;

namespace GG.Season
{
    public static class ScheduleService
    {
        public static List<GameInfo>[] Generate(List<string> teamAbbrs, int weeks = 14)
        {
            if (teamAbbrs == null) return Array.Empty<List<GameInfo>>();
            var rng = new Random(SeasonState.SeasonStartSeed);
            var teams = teamAbbrs.Where(t => !string.IsNullOrEmpty(t)).ToList();
            if (teams.Count % 2 == 1) teams.Add("BYE");

            // shuffle initial order
            for (int i = teams.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (teams[i], teams[j]) = (teams[j], teams[i]);
            }

            int totalWeeks = Math.Min(weeks, teams.Count - 1);
            var schedule = new List<GameInfo>[totalWeeks];
            var rotation = new List<string>(teams);

            for (int w = 0; w < totalWeeks; w++)
            {
                var games = new List<GameInfo>();
                int half = rotation.Count / 2;
                for (int i = 0; i < half; i++)
                {
                    var home = rotation[i];
                    var away = rotation[rotation.Count - 1 - i];
                    if (home == "BYE" || away == "BYE") continue;
                    bool swap = ((i + w) % 2 == 0);
                    var g = new GameInfo
                    {
                        week = w + 1,
                        home = swap ? home : away,
                        away = swap ? away : home
                    };
                    games.Add(g);
                }
                schedule[w] = games;

                // rotate teams except first
                var last = rotation[rotation.Count - 1];
                rotation.RemoveAt(rotation.Count - 1);
                rotation.Insert(1, last);
            }
            return schedule;
        }
    }
}
