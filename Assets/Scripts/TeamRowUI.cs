using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using GridironGM.Data;

namespace GridironGM.UI
{
public class TeamRowUI : MonoBehaviour, IPointerClickHandler
{
    public Image logoImage;
    public TMP_Text nameText;
    public TMP_Text conferenceText;

    public string teamAbbreviation;
    public System.Action OnRowClicked;

    // Existing method kept for backward compatibility
    public void SetData(TeamDataUI data)
    {
        if (data == null)
        {
            Debug.LogWarning("TeamRowUI.SetData called with null data!");
            return;
        }

        if (logoImage != null)
        {
            logoImage.sprite = data.logo;
        }

        if (nameText != null)
        {
            nameText.text = data.teamName;
        }

        if (conferenceText != null)
        {
            conferenceText.text = data.teamConference;
        }

        teamAbbreviation = data.abbreviation;
    }

    // New API for setting data from TeamData model
    public void Set(TeamData data, System.Action onClick)
    {
        if (data == null) return;

        if (nameText != null)
        {
            nameText.text = $"{data.city} {data.name}";
        }

        if (conferenceText != null)
        {
            conferenceText.text = data.conference;
        }

        teamAbbreviation = data.abbreviation;
        OnRowClicked = onClick;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"TeamRowUI clicked: {teamAbbreviation}");
        OnRowClicked?.Invoke();
    }
}
}
