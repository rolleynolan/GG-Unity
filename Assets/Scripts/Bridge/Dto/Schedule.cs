using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GG.Bridge.Dto
{
    [Serializable]
    public class GameDTO
    {
        [JsonProperty("game_id")] public string GameId;
        [JsonProperty("home")]    public string Home;
        [JsonProperty("away")]    public string Away;
        [JsonProperty("played")]  public bool Played;
        [JsonProperty("score")]   public Dictionary<string,int> Score = new(); // { "ATL": 0, "PHI": 0 }
    }

    [Serializable]
    public class WeekDTO
    {
        [JsonProperty("week")]  public int Week;
        [JsonProperty("games")] public List<GameDTO> Games = new();
    }

    [Serializable]
    public class ScheduleDTO
    {
        [JsonProperty("api_version")] public string ApiVersion = "gg.v1";
        [JsonProperty("year")]        public int Year;
        [JsonProperty("weeks")]       public List<WeekDTO> Weeks = new();
    }
}
