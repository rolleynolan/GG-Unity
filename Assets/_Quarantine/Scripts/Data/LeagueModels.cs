using System;

[Serializable] public class Team { public string city; public string name; public string conference; public string abbreviation; }
[Serializable] public class TeamList { public Team[] teams; }

[Serializable] public class Player {
    public string id; public string name; public string position; public int overall; public int number;
}

[Serializable] public class TeamGame {
    public int week; public string opponent; public bool home; public string time; public string result; // e.g., "W 24-17" or ""
}
