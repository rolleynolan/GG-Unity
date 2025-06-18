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

    // Make sure to define TeamDataUI in your project, for example:
    public class TeamDataUI
    {
        // Example fields for TeamDataUI
        public Sprite logo;
        public string teamName;
        public string teamConference;
        public string abbreviation;
    }

    public void SetData(TeamDataUI data)
    {
        if (data == null) return;
        if (logoImage != null) logoImage.sprite = data.logo;
        if (nameText != null) nameText.text = data.teamName;
        if (conferenceText != null) conferenceText.text = data.teamConference;
        teamAbbreviation = data.abbreviation;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnRowClicked?.Invoke();
    }
}
