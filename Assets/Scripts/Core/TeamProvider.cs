using System; using System.Collections; using System.Collections.Generic;
using System.IO; using System.Linq; using System.Reflection; using System.Text.RegularExpressions;

public interface ITeamProvider { List<string> GetAllTeamAbbrs(); }

public sealed class TeamProvider : ITeamProvider {
  static readonly Regex AbbrProp = new Regex("\"abbr\"\\s*:\\s*\"([A-Za-z_-]+)\"", RegexOptions.IgnoreCase|RegexOptions.Compiled);
  static readonly Regex KeyObj   = new Regex("^\\s*\"([A-Za-z0-9_-]{2,6})\"\\s*:\\s*\\{", RegexOptions.Multiline|RegexOptions.Compiled);

  public List<string> GetAllTeamAbbrs() {
    var a = FromJsonProps(); if (a.Count>1) { GGLog.Info($"TeamProvider props={a.Count}"); return Dedup(a); }
    var b = FromJsonKeys();  if (b.Count>1) { GGLog.Info($"TeamProvider keys={b.Count}"); return Dedup(b); }
    var c = FromRepo();      if (c.Count>1) { GGLog.Info($"TeamProvider repo={c.Count}"); return Dedup(c); }
    GGLog.Warn("TeamProvider: no ABBRs discovered; falling back to selected only.");
    return new List<string>();
  }

  List<string> FromJsonProps() {
    try {
      var p = GGPaths.Streaming(GGConventions.TeamsJsonFile); if (!File.Exists(p)) return new();
      var txt = File.ReadAllText(p);
      var ms = AbbrProp.Matches(txt);
      var list = new List<string>(ms.Count); foreach (Match m in ms) list.Add(m.Groups[1].Value);
      return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    } catch { return new(); }
  }
  List<string> FromJsonKeys() {
    try {
      var p = GGPaths.Streaming(GGConventions.TeamsJsonFile); if (!File.Exists(p)) return new();
      var txt = File.ReadAllText(p);
      var ms = KeyObj.Matches(txt);
      var list = new List<string>(ms.Count);
      foreach (Match m in ms) { var k=m.Groups[1].Value; if (!LikelyNonTeam(k)) list.Add(k); }
      return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    } catch { return new(); }
  }
  static bool LikelyNonTeam(string k){
    var s=k.ToLowerInvariant(); return s is "teams" or "meta" or "settings" or "logos" or "version";
  }

  List<string> FromRepo() {
    try {
      var t = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a=>{ try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
        .FirstOrDefault(x=>x.Name=="LeagueRepository");
      if (t==null) return new();
      var inst = t.GetProperty("Instance",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static)?.GetValue(null)
              ?? t.GetProperty("Current", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static)?.GetValue(null);
      var flags = BindingFlags.Public|BindingFlags.NonPublic|((inst==null)?BindingFlags.Static:BindingFlags.Instance);
      var sink = new List<string>();
      foreach (var m in t.GetMembers(flags)) {
        object val=null; try {
          if (m is PropertyInfo p && p.CanRead) val=p.GetValue(inst);
          else if (m is FieldInfo f) val=f.GetValue(inst);
        } catch { }
        if (val is IDictionary dict) foreach (var k in dict.Keys) if (k is string s && LooksLikeAbbr(s)) sink.Add(s);
        if (val is IEnumerable seq && val is not string)
          foreach (var it in seq) { var a=ExtractAbbr(it); if (!string.IsNullOrEmpty(a)) sink.Add(a); }
      }
      return sink.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    } catch { return new(); }
  }

  static string ExtractAbbr(object o){ if (o==null) return null; var t=o.GetType();
    var p=t.GetProperty("abbr")??t.GetProperty("Abbr")??t.GetProperty("id")??t.GetProperty("ID");
    if (p!=null){ var v=p.GetValue(o)?.ToString(); if (LooksLikeAbbr(v)) return v; }
    var f=t.GetField("abbr")??t.GetField("Abbr")??t.GetField("id")??t.GetField("ID");
    if (f!=null){ var v=f.GetValue(o)?.ToString(); if (LooksLikeAbbr(v)) return v; }
    return null; }

  static bool LooksLikeAbbr(string s){ if (string.IsNullOrWhiteSpace(s)) return false; s=s.Trim();
    if (s.Length<2||s.Length>6) return false; int letters=0;
    foreach (var c in s){ if (char.IsLetter(c)) letters++; else if (c!='-'&&c!='_') return false; }
    return letters>=2; }

  static List<string> Dedup(List<string> x)=>x.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
}
