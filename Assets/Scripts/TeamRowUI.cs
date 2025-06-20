using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TeamRowUI : MonoBehaviour, IPointerClickHandler
{
    public Image logoImage;
    public TMP_Text nameText;
    public TMP_Text conferenceText;

    public string teamAbbreviation;
    public System.Action OnRowClicked;

    // Data for populating the row. Defined in TeamDataUI.cs

    public void SetData(TeamDataUI data)
    {
        if (data == null)
        {
            Debug.LogWarning("TeamRowUI.SetData called with null data!");
            return;
        }

        Debug.Log($"Applying Team Data: {data.teamName}, {data.teamConference}, {data.abbreviation}");

        if (logoImage != null)
        {
            logoImage.sprite = data.logo;
            Debug.Log($"Logo assigned: {data.logo}");
        }
        else
        {
            Debug.LogWarning("Logo Image reference is missing on TeamRowUI.");
        }

        if (nameText != null)
        {
            nameText.text = data.teamName;
            Debug.Log($"Name text set: {data.teamName}");
        }
        else
        {
            Debug.LogWarning("Name Text reference is missing on TeamRowUI.");
        }

        if (conferenceText != null)
        {
            conferenceText.text = data.teamConference;
            Debug.Log($"Conference text set: {data.teamConference}");
        }
        else
        {
            Debug.LogWarning("Conference Text reference is missing on TeamRowUI.");
        }

        teamAbbreviation = data.abbreviation;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"TeamRowUI clicked: {teamAbbreviation}");
        OnRowClicked?.Invoke();
    }
}
