using GG.Infra;

public static class GGLog {
  public static void Info(string m)  => GG.Infra.GGLog.Info(m);
  public static void Warn(string m)  => GG.Infra.GGLog.Warn(m);
  public static void Error(string m) => GG.Infra.GGLog.Error(m);
}
