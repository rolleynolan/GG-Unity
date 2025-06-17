using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class ScheduleUI : MonoBehaviour
{
    public GameObject weekRowPrefab;
    public Transform contentParent;
    public GameObject schedulePanel;
    public GameObject dashboardPanel;

    void OnEnable()
    {
        StartCoroutine(LoadRealSchedule());
    }

    IEnumerator LoadRealSchedule()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        string path = Path.Combine(Application.streamingAssetsPath, "schedule_by_team.json");
        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load schedule: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        JObject root = JObject.Parse(json);
        string team = PlayerPrefs.GetString("selected_team", "BUF");

        if (!root.ContainsKey(team))
        {
            Debug.LogError("Team not found in schedule JSON: " + team);
            yield break;
        }

        foreach (var game in root[team])
        {
            int week = (int)game["week"];
            string opponent = (string)game["opponent"];
            bool home = (bool)game["home"];
            string label = $"Week {week}: {(home ? "vs" : "@")} {opponent}";

            GameObject row = Instantiate(weekRowPrefab, contentParent);
            row.transform.Find("WeekText").GetComponent<Text>().text = label;
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
