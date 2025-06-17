using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DashboardUI : MonoBehaviour
{
    public Text teamNameText;
    public Text weekText;
    public Text phaseText;
    public Text opponentText;
    public Text recordText;

    void Start()
    {
        string team = PlayerPrefs.GetString("selected_team", "Unknown Team");
        if (teamNameText != null)
            teamNameText.text = "Team: " + team;
        if (weekText != null)
            weekText.text = "Week: 1";
        if (phaseText != null)
            phaseText.text = "Phase: Preseason";
        if (opponentText != null)
            opponentText.text = "Next Opponent: TBD";
        if (recordText != null)
            recordText.text = "Record: 0-0";
    }

    public void OnViewRosterPressed()
    {
        Debug.Log("View Roster pressed");
    }

    public void OnViewDepthChartPressed()
    {
        Debug.Log("View Depth Chart pressed");
    }

    public void OnViewSchedulePressed()
    {
        Debug.Log("View Schedule pressed");
    }

    public void OnSimWeekPressed()
    {
        Debug.Log("Sim Week pressed (future sim logic)");
    }

    public void OnMainMenuPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
