using System;
using System.Collections.Generic;

[Serializable]
public class TeamData
{
    public string city;
    public string name;
    public string abbreviation;
    public string conference;
    public string division;
    public string id;
}

[Serializable]
public class TeamDataList
{
    public List<TeamData> teams;
}
