using UnityEngine;
public static class TeamProvider {
    const string Key="SelectedTeamAbbr";
    public static string SelectedAbbr { get=>PlayerPrefs.GetString(Key,string.Empty);
        set{ PlayerPrefs.SetString(Key,value); PlayerPrefs.Save(); } }
    public static string GetSelectedTeamAbbreviation()=>SelectedAbbr;
    public static void   SetSelectedTeamAbbreviation(string abbr)=>SelectedAbbr=abbr;
}
