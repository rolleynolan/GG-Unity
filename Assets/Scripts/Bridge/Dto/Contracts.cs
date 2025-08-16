using System;
using System.Collections.Generic;
using UnityEngine;

namespace System.Text.Json.Serialization {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  internal sealed class JsonPropertyNameAttribute : Attribute {
    public JsonPropertyNameAttribute(string n) { }
  }
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  internal sealed class JsonExtensionDataAttribute : Attribute { }
}

[Serializable]
public class ContractYearTerm {
  [JsonPropertyName("year")]
  public int Year;
  [JsonPropertyName("base")]
  public long Base;
  [JsonPropertyName("signingProrated")]
  public long SigningProrated;
  [JsonPropertyName("rosterBonus")]
  public long RosterBonus;
  [JsonPropertyName("workoutBonus")]
  public long WorkoutBonus;
  [JsonPropertyName("guaranteedBase")]
  public long GuaranteedBase;
}

[Serializable]
public class Guarantee {
  [JsonPropertyName("type")]
  public string Type;
  [JsonPropertyName("throughYear")]
  public int ThroughYear;
}

[Serializable]
public class Incentive {
  [JsonPropertyName("type")]
  public string Type;
  [JsonPropertyName("amount")]
  public long Amount;
  [JsonPropertyName("metric")]
  public string Metric;
  [JsonPropertyName("threshold")]
  public string Threshold;
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
  [SerializeField] List<TKey> keys = new List<TKey>();
  [SerializeField] List<TValue> values = new List<TValue>();

  public void OnBeforeSerialize() {
    keys.Clear(); values.Clear();
    foreach (var kv in this) { keys.Add(kv.Key); values.Add(kv.Value); }
  }

  public void OnAfterDeserialize() {
    this.Clear();
    for (int i = 0; i < Math.Min(keys.Count, values.Count); i++) this[keys[i]] = values[i];
  }
}

[Serializable]
public class ContractDTO {
  [JsonPropertyName("apiVersion")]
  public string ApiVersion;
  [JsonPropertyName("startYear")]
  public int StartYear;
  [JsonPropertyName("endYear")]
  public int EndYear;
  [JsonPropertyName("terms")]
  public List<ContractYearTerm> Terms;
  [JsonPropertyName("guarantees")]
  public List<Guarantee> Guarantees;
  [JsonPropertyName("incentives")]
  public List<Incentive> Incentives;
  [JsonPropertyName("flags")]
  public SerializableDictionary<string, bool> Flags;
  [JsonPropertyName("notes")]
  public List<string> Notes;
  [JsonExtensionData]
  public SerializableDictionary<string, string> Extra;
}
