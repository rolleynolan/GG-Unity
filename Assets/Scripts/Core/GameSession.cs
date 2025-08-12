using UnityEngine;

public static class GameSession
{
    const string KeyTeam = "GG.SelectedTeamAbbr";
    const string KeyWeek = "GG.CurrentWeek";

    public static string SelectedTeamAbbr
    {
        get => PlayerPrefs.GetString(KeyTeam, "ATL"); // default any
        set { PlayerPrefs.SetString(KeyTeam, value); PlayerPrefs.Save(); }
    }

    public static int CurrentWeek
    {
        get => PlayerPrefs.GetInt(KeyWeek, 1);
        set { PlayerPrefs.SetInt(KeyWeek, Mathf.Max(1, value)); PlayerPrefs.Save(); }
    }
}
