using System.Collections.Generic;
using UnityEngine;

public static class TeamDatabase
{
    public static List<TeamData> GetAllTeams()
    {
        return new List<TeamData>
        {
            new TeamData { abbreviation = "ATL", teamName = "Atlanta", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "BAL", teamName = "Baltimore", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "BOS", teamName = "Boston", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "BUF", teamName = "Buffalo", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "CLT", teamName = "Charlotte", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "CHI", teamName = "Chicago", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "CIN", teamName = "Cincinnati", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "CLE", teamName = "Cleveland", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "DAL", teamName = "Dallas", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "DEN", teamName = "Denver", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "DET", teamName = "Detroit", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "GB",  teamName = "Green Bay", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "HOU", teamName = "Houston", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "IND", teamName = "Indianapolis", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "KC",  teamName = "Kansas City", conference = "Conference A", logo = null },
            new TeamData { abbreviation = "LV",  teamName = "Las Vegas", conference = "Conference A", logo = null },

            new TeamData { abbreviation = "LA",  teamName = "Los Angeles", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "MIA", teamName = "Miami", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "MIN", teamName = "Minneapolis", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "NO",  teamName = "New Orleans", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "NY",  teamName = "New York", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "ORL", teamName = "Orlando", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "PHI", teamName = "Philadelphia", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "PHX", teamName = "Phoenix", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "PIT", teamName = "Pittsburgh", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "POR", teamName = "Portland", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "SD",  teamName = "San Diego", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "SF",  teamName = "San Francisco", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "SEA", teamName = "Seattle", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "TB",  teamName = "Tampa Bay", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "TEN", teamName = "Tennessee", conference = "Conference B", logo = null },
            new TeamData { abbreviation = "WAS", teamName = "Washington", conference = "Conference B", logo = null }
        };
    }
}
