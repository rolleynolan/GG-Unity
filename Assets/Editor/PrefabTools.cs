#if UNITY_EDITOR
using UnityEditor; using UnityEngine;
public static class PrefabTools {
  [MenuItem("GridironGM/Dev/Scan & Remove Missing Scripts")]
  public static void RemoveMissing() {
    var guids = AssetDatabase.FindAssets("t:Prefab");
    int removed=0, scanned=0;
    foreach (var g in guids) {
      var path = AssetDatabase.GUIDToAssetPath(g);
      var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (!prefab) continue;
      scanned++; removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
    }
    Debug.Log($"[GG] Prefabs scanned:{scanned}, missing scripts removed:{removed}");
  }

  [MenuItem("GridironGM/Dev/Clear Season Save")]
  public static void ClearSeason() {
    var p = GGPaths.Save(GGConventions.SeasonSaveFile);
    if (System.IO.File.Exists(p)) { System.IO.File.Delete(p); Debug.Log($"[GG] Deleted {p}"); }
    else Debug.Log("[GG] No season save found.");
  }
}
#endif
