using System;
using System.Collections.Generic;

[Serializable]
public class TeamData
{
    public string city;
    public string name;
    public string conference;
    public string abbreviation;
}

[Serializable]
public class TeamDataList
{
    public List<TeamData> teams = new();
}
