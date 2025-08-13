using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamRowBinder : MonoBehaviour
{
    public TMP_Text NameText;    // e.g., "Washington Generals (WAS)"
    public TMP_Text ConfText;    // e.g., "Conference"
    public Image LogoImage;

    void EnsureLayout()
    {
        // Ensure a HorizontalLayoutGroup exists so texts don’t overlap.
        var h = GetComponent<HorizontalLayoutGroup>();
        if (!h)
        {
            h = gameObject.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft;
            h.spacing = 12f;
            h.childForceExpandWidth = true;
        }

        var le = GetComponent<LayoutElement>();
        if (!le) le = gameObject.AddComponent<LayoutElement>();
        le.minHeight = 48f;
    }

    public void Set(TeamData t)
    {
        EnsureLayout();
        if (NameText) NameText.text = $"{t.city} {t.name} ({t.abbreviation})";
        if (ConfText) ConfText.text = t.conference;
        // LogoImage is optional; you already load sprites elsewhere.
    }
}
