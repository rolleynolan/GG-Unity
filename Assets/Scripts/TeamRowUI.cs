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

    public void SetData(TeamDataUI data)
    {
        if (data == null) return;
        if (logoImage != null) logoImage.sprite = data.logo;
        if (nameText != null) nameText.text = data.teamName;
        if (conferenceText != null) conferenceText.text = data.conference;
        teamAbbreviation = data.abbreviation;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnRowClicked?.Invoke();
    }
}
