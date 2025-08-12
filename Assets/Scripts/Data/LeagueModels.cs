using System;

[Serializable] public class Team { public string city; public string name; public string conference; public string abbreviation; }
[Serializable] public class TeamList { public Team[] teams; }

[Serializable] public class Player {
    public string id; public string name; public string position; public int overall; public int number;
}

// rosters_by_team.json: { "ATL": [Player, ...], "BUF": [Player, ...], ... }
[Serializable] public class RosterByTeam { public SerializableDict<string, Player[]> data; }

[Serializable] public class TeamGame {
    public int week; public string opponent; public bool home; public string time; public string result; // e.g., "W 24-17" or ""
}
// schedule_by_team.json: { "ATL": [TeamGame, ...], "BUF": [TeamGame, ...], ... }
[Serializable] public class ScheduleByTeam { public SerializableDict<string, TeamGame[]> data; }

// Simple serializable dict wrapper
[Serializable] public class SerializableDict<TKey, TValue> { public System.Collections.Generic.List<TKey> keys; public System.Collections.Generic.List<TValue> values; }
