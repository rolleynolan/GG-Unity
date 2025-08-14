using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamRowBinder : MonoBehaviour
{
    public TMP_Text NameText;    // "Washington Generals (WAS)"
    public TMP_Text ConfText;    // "Conference"
    public Image LogoImage;      // optional

    public void AutoWireIfNeeded()
    {
        if (!NameText) NameText = FindText("NameText");
        if (!ConfText) ConfText = FindText("ConfText");
        if (!NameText || !ConfText)
        {
            var all = GetComponentsInChildren<TMP_Text>(true);
            if (all.Length >= 2)
            {
                if (!NameText) NameText = all[0];
                if (!ConfText) ConfText = all[1];
            }
        }
    }

    TMP_Text FindText(string child)
    {
        var t = transform.Find(child);
        return t ? t.GetComponent<TMP_Text>() : null;
    }

    public void Set(TeamData t)
    {
        AutoWireIfNeeded();
        if (NameText) NameText.text = $"{t.city} {t.name} ({t.abbreviation})";
        if (ConfText) ConfText.text = t.conference;
        // LogoImage stays as-is (you load sprites elsewhere)
    }
}
