using System;
using System.Collections.Generic;

namespace GG.Bridge.Dto
{
    [Serializable]
    public class GameDTO
    {
        public string game_id;
        public string home;
        public string away;
        public bool played;
        public int homePts;
        public int awayPts;
    }

    [Serializable]
    public class WeekDTO
    {
        public int week;
        public List<GameDTO> games = new List<GameDTO>();
    }

    [Serializable]
    public class ScheduleDTO
    {
        public string api_version = "gg.v1";
        public int year;
        public List<WeekDTO> weeks = new List<WeekDTO>();
    }
}
