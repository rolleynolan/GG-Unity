using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScheduleUI : MonoBehaviour
{
    public GameObject weekRowPrefab;
    public Transform contentParent;
    public GameObject schedulePanel;
    public GameObject dashboardPanel;

    void OnEnable()
    {
        PopulateFakeSchedule();
    }

    void PopulateFakeSchedule()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<string> fakeSchedule = new List<string>
        {
            "Week 1: vs TBD",
            "Week 2: @ TBD",
            "Week 3: vs TBD",
            "Week 4: @ TBD",
            "Week 5: vs TBD",
            "Week 6: @ TBD",
            "Week 7: vs TBD",
            "Week 8: @ TBD",
            "Week 9: vs TBD",
            "Week 10: @ TBD"
        };

        foreach (string week in fakeSchedule)
        {
            GameObject row = Instantiate(weekRowPrefab, contentParent);
            row.transform.Find("WeekText").GetComponent<Text>().text = week;
        }
    }

    public void OnSimWeekPressed()
    {
        Debug.Log("Sim Week clicked (future logic)");
    }

    public void OnBackPressed()
    {
        schedulePanel.SetActive(false);
        dashboardPanel.SetActive(true);
    }
}
