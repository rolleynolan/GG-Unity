using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GG.Game // <- match your project namespace if you have one
{
    [Serializable] public struct TeamRecord { public int W, L, PF, PA; }

    [Serializable] public struct GameInfo
    {
        public int week;
        public string home, away;
    }

    [Serializable] public struct GameResult
    {
        public int week;
        public string home, away;
        public int homeScore, awayScore;
    }

    // Wrapper so JsonUtility can serialize weeks and their games
    [Serializable] public class WeekGames
    {
        public int week;
        public List<GameInfo> games = new List<GameInfo>();
    }

    // Wrapper so JsonUtility can serialize records (dictionaries are not supported)
    [Serializable] public class TeamRecordEntry
    {
        public string abbr;
        public TeamRecord rec;
    }

    [Serializable]
    public class SeasonState
    {
        public int week = 1;                                            // 1-based
        public List<WeekGames> schedule = new List<WeekGames>();        // [week]->games
        public List<GameResult> results = new List<GameResult>();       // played games
        public List<TeamRecordEntry> records = new List<TeamRecordEntry>(); // serialized map

        static string SavePath => Path.Combine(Application.persistentDataPath, "season.json");

        /// Load season from disk or create a fresh season with a generated schedule.
        public static SeasonState LoadOrCreate(string selectedAbbr, List<string> allAbbrs, Func<List<string>, List<WeekGames>> scheduleGen)
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    var json = File.ReadAllText(SavePath);
                    var loaded = JsonUtility.FromJson<SeasonState>(json);
                    if (loaded != null && loaded.schedule != null && loaded.schedule.Count > 0)
                        return loaded;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SeasonState] Load failed: {e.Message}");
            }

            // New season
            var s = new SeasonState();
            s.week = 1;
            s.schedule = scheduleGen != null ? scheduleGen(allAbbrs) : new List<WeekGames>();
            s.records = new List<TeamRecordEntry>(allAbbrs.Count);
            foreach (var ab in allAbbrs) s.records.Add(new TeamRecordEntry { abbr = ab, rec = new TeamRecord() });
            s.results = new List<GameResult>();
            s.Save();
            return s;
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(this, true);
            File.WriteAllText(SavePath, json);
        }

        // ------- Records helpers (convert to/from map) -------

        Dictionary<string, TeamRecord> ToMap()
        {
            var map = new Dictionary<string, TeamRecord>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in records) map[e.abbr] = e.rec;
            return map;
        }

        void FromMap(Dictionary<string, TeamRecord> map)
        {
            records.Clear();
            foreach (var kv in map) records.Add(new TeamRecordEntry { abbr = kv.Key, rec = kv.Value });
        }

        // ------- API used by UI/Sim -------

        public GameInfo? GetNextGame(string abbr)
        {
            for (int w = week; w <= schedule.Count; w++)
            {
                var wg = schedule[w - 1];
                foreach (var g in wg.games)
                {
                    if (!g.home.Equals(abbr, StringComparison.OrdinalIgnoreCase) &&
                        !g.away.Equals(abbr, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Skip if already have a result
                    if (results.Exists(r => r.week == g.week && r.home == g.home && r.away == g.away))
                        continue;

                    return g;
                }
            }
            return null;
        }

        public void ApplyResult(GameResult r)
        {
            // de-dup (resim same game)
            results.RemoveAll(x => x.week == r.week && x.home == r.home && x.away == r.away);
            results.Add(r);

            var map = ToMap();
            if (!map.TryGetValue(r.home, out var home)) home = new TeamRecord();
            if (!map.TryGetValue(r.away, out var away)) away = new TeamRecord();

            home.PF += r.homeScore; home.PA += r.awayScore;
            away.PF += r.awayScore; away.PA += r.homeScore;

            if (r.homeScore > r.awayScore) { home.W++; away.L++; }
            else                           { away.W++; home.L++; }

            map[r.home] = home;
            map[r.away] = away;
            FromMap(map);
        }
    }
}
