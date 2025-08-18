using TMPro;
using UnityEngine;

public class PlayerRowBinder : MonoBehaviour
{
    public TMP_Text NameText, PosText, OvrText, AgeText;

    public void AutoWireIfNeeded()
    {
        if (!NameText) NameText = FindText("NameText");
        if (!PosText)  PosText  = FindText("PosText");
        if (!OvrText)  OvrText  = FindText("OvrText");
        if (!AgeText)  AgeText  = FindText("AgeText");

        if (!NameText || !PosText || !OvrText || !AgeText)
        {
            var all = GetComponentsInChildren<TMP_Text>(true);
            if (all.Length >= 4)
            {
                if (!NameText) NameText = all[0];
                if (!PosText)  PosText  = all[1];
                if (!OvrText)  OvrText  = all[2];
                if (!AgeText)  AgeText  = all[3];
            }
        }
    }

    TMP_Text FindText(string childName)
    {
        var t = transform.Find(childName);
        return t ? t.GetComponent<TMP_Text>() : null;
    }

    public void Bind(PlayerDTO p)
    {
        AutoWireIfNeeded();
        if (NameText) NameText.text = $"{p.first_name} {p.last_name}";
        if (PosText)  PosText.text  = p.pos;
        if (OvrText)  OvrText.text  = p.overall.ToString();
        if (AgeText)  AgeText.text  = p.age.ToString();
    }
}
