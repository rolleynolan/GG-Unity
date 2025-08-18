using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamRowUI : MonoBehaviour
{
    [Header("Refs")]
    public Image logoImage;
    public TMP_Text nameText;
    public TMP_Text conferenceText;
    public Button button;

    public void Set(TeamData data, System.Action onClick)
    {
        AutoWireIfNeeded();

        if (nameText)      nameText.text = $"{data.city} {data.name} ({data.abbreviation})";
        if (conferenceText) conferenceText.text = data.conference;

        if (button)
        {
            button.onClick.RemoveAllListeners();
            if (onClick != null) button.onClick.AddListener(() => onClick());
        }

        if (logoImage)
        {
            var spr = LogoService.Get(data.abbreviation);
            logoImage.enabled = spr != null;
            logoImage.sprite = spr;
            logoImage.preserveAspect = true;
        }
    }

    void AutoWireIfNeeded()
    {
        if (!button) button = GetComponent<Button>();
        if (!logoImage) logoImage = GetComponentInChildren<Image>(true);
        if (!nameText || !conferenceText)
        {
            var tmps = GetComponentsInChildren<TMP_Text>(true);
            if (tmps.Length >= 2)
            {
                if (!nameText) nameText = tmps[0];
                if (!conferenceText) conferenceText = tmps[1];
            }
        }
    }
}
