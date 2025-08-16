using System;
using System.IO;
using UnityEngine;

public static class DataIO {
  public static T LoadJson<T>(string relativePath){
    var path = GGPaths.Project(relativePath);
    GGLog.Info($"LoadJson {path}");
    try {
      var txt = File.ReadAllText(path);
      return JsonUtility.FromJson<T>(txt);
    } catch (Exception ex) {
      GGLog.Error($"LoadJson failed: {ex.Message}");
      throw;
    }
  }
}
