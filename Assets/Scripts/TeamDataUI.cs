using UnityEngine;

[System.Serializable]
public class TeamDataUI
{
    public string teamName;
    public string conference;
    public string abbreviation;
    public Sprite logo;

    public TeamDataUI(string teamName, string conference, string abbreviation, Sprite logo)
    {
        this.teamName = teamName;
        this.conference = conference;
        this.abbreviation = abbreviation;
        this.logo = logo;
    }
}
