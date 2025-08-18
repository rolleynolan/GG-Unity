#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class DevTeamUtilities
{
    [MenuItem("GridironGM/Dev/Clear Selected Team")]
    public static void ClearSelectedTeam()
    {
        PlayerPrefs.DeleteKey("selected_team");
        PlayerPrefs.Save();
        Debug.Log("[Dev] Cleared PlayerPrefs 'selected_team'.");
    }
}
#endif
