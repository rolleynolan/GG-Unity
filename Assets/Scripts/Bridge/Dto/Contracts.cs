using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GG.Bridge.Dto
{
    [Serializable]
    public class ContractYearTerm
    {
        [JsonProperty("year")] public int Year;
        [JsonProperty("base")] public long Base;
        [JsonProperty("signingProrated")] public long SigningProrated;
        [JsonProperty("rosterBonus")] public long RosterBonus;
        [JsonProperty("workoutBonus")] public long WorkoutBonus;
        [JsonProperty("guaranteedBase")] public long GuaranteedBase;
    }

    [Serializable]
    public class Guarantee
    {
        [JsonProperty("type")] public string Type;
        [JsonProperty("throughYear")] public int ThroughYear;
    }

    [Serializable]
    public class Incentive
    {
        [JsonProperty("type")] public string Type;
        [JsonProperty("amount")] public long Amount;
        [JsonProperty("metric")] public string Metric;
        [JsonProperty("threshold")] public string Threshold;
    }

    [Serializable]
    public class ContractDTO
    {
        [JsonProperty("api_version")] public string ApiVersion;
        [JsonProperty("startYear")] public int StartYear;
        [JsonProperty("endYear")] public int EndYear;

        [JsonProperty("terms")] public List<ContractYearTerm> Terms = new();
        [JsonProperty("guarantees")] public List<Guarantee> Guarantees = new();
        [JsonProperty("incentives")] public List<Incentive> Incentives = new();
        [JsonProperty("flags")] public Dictionary<string, bool> Flags = new();
        [JsonProperty("notes")] public List<string> Notes = new();

        // Forward-compat: keep unknown fields instead of crashing
        [JsonExtensionData] public IDictionary<string, JToken> Extra { get; set; }
    }
}
