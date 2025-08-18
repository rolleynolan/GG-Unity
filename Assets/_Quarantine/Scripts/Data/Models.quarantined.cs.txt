using System;
using System.Collections.Generic;

[Serializable]
public class PlayerDTO
{
    public string first_name;
    public string last_name;
    public string pos;
    public int overall;
    public int age;
}

[Serializable]
public class TeamRosterDTO
{
    public string abbreviation;
    public List<PlayerDTO> players = new();
}

[Serializable]
public class RostersRoot
{
    public List<TeamRosterDTO> teams = new();
}
