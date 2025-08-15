using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using UnityEngine;

namespace GG.Season
{
    [Serializable]
    public class SeasonState
    {
        public const int SeasonStartSeed = 20240101;

        public int week;
        public Dictionary<string, TeamRecord> records = new();
        public List<GameInfo>[] scheduleByWeek;
        public List<GameResult> results = new();

        static string SavePath => Path.Combine(Application.persistentDataPath, "season.json");

        public static SeasonState LoadOrCreate(string selectedAbbr)
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    var json = File.ReadAllText(SavePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var loaded = JsonSerializer.Deserialize<SeasonState>(json, options);
                    if (loaded != null)
                        return loaded;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SeasonState] Load failed: {e}");
            }

            var teamAbbrs = global::LeagueRepository.GetTeams()
                .Where(t => !string.IsNullOrEmpty(t.abbreviation))
                .Select(t => t.abbreviation)
                .ToList();

            var state = new SeasonState();
            state.week = 1;
            state.records = teamAbbrs.ToDictionary(a => a, a => new TeamRecord());
            state.scheduleByWeek = ScheduleService.Generate(teamAbbrs, 14);
            state.Save();
            return state;
        }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SeasonState] Save failed: {e}");
            }
        }

        public GameInfo? GetNextGame(string abbr)
        {
            if (string.IsNullOrEmpty(abbr) || scheduleByWeek == null) return null;
            int w = week - 1;
            if (w < 0 || w >= scheduleByWeek.Length) return null;
            foreach (var g in scheduleByWeek[w])
            {
                bool played = results.Exists(r => r.week == g.week && r.home == g.home && r.away == g.away);
                if (!played && (g.home == abbr || g.away == abbr))
                    return g;
            }
            return null;
        }

        public void ApplyResult(GameResult r)
        {
            results.Add(r);

            if (!records.ContainsKey(r.home)) records[r.home] = new TeamRecord();
            if (!records.ContainsKey(r.away)) records[r.away] = new TeamRecord();

            var home = records[r.home];
            var away = records[r.away];

            home.PF += r.homeScore;
            home.PA += r.awayScore;
            away.PF += r.awayScore;
            away.PA += r.homeScore;

            if (r.homeScore > r.awayScore) { home.W++; away.L++; }
            else { away.W++; home.L++; }

            records[r.home] = home;
            records[r.away] = away;
        }
    }
}
