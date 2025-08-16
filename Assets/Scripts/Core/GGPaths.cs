using System.IO; using UnityEngine;
public static class GGPaths {
  public static string Streaming(string file)=>Path.Combine(Application.streamingAssetsPath,file);
  public static string Save(string file)=>Path.Combine(Application.persistentDataPath,file);
}
