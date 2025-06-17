using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RosterUI : MonoBehaviour
{
    public GameObject playerRowPrefab; // Assign in inspector
    public Transform contentParent;    // ScrollView Content parent
    public GameObject dashboardPanel;
    public GameObject rosterPanel;

    void OnEnable()
    {
        PopulateFakeRoster();
    }

    void PopulateFakeRoster()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<Player> fakePlayers = new List<Player>
        {
            new Player("QB", "J. Allen", 92),
            new Player("RB", "D. Cook", 85),
            new Player("WR", "S. Diggs", 90),
            new Player("LT", "T. Dawkins", 83),
            new Player("CB", "T. White", 88),
        };

        foreach (var p in fakePlayers)
        {
            GameObject row = Instantiate(playerRowPrefab, contentParent);
            row.transform.Find("PositionText").GetComponent<Text>().text = p.position;
            row.transform.Find("NameText").GetComponent<Text>().text = p.name;
            row.transform.Find("OverallText").GetComponent<Text>().text = $"OVR: {p.overall}";
        }
    }

    public void OnBackPressed()
    {
        rosterPanel.SetActive(false);
        dashboardPanel.SetActive(true);
    }

    private class Player
    {
        public string position;
        public string name;
        public int overall;

        public Player(string pos, string name, int ovr)
        {
            this.position = pos;
            this.name = name;
            this.overall = ovr;
        }
    }
}
