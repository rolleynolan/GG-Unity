using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamRowBinder : MonoBehaviour
{
    public Image LogoImage;
    public TMP_Text NameText;
    public TMP_Text ConfText;

    public void Set(TeamData t)
    {
        AutoWireIfNeeded();
        if (NameText) NameText.text = $"{t.city} {t.name}";
        if (ConfText) ConfText.text = t.conference;
        var spr = LogoService.Get(t.abbreviation);
        if (LogoImage)
        {
            LogoImage.enabled = spr != null;
            LogoImage.sprite = spr;
            LogoImage.preserveAspect = true;
        }
    }

    void AutoWireIfNeeded()
    {
        if (!LogoImage) LogoImage = GetComponentInChildren<Image>(true);
        if (!NameText || !ConfText)
        {
            var texts = GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length >= 2)
            {
                if (!NameText) NameText = texts[0];
                if (!ConfText) ConfText = texts[1];
            }
        }
    }
}
