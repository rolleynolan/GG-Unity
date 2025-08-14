using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PlayerRowLayout : MonoBehaviour
{
    [Header("Columns")]
    public TMP_Text nameText;
    public TMP_Text posText;
    public TMP_Text ovrText;
    public TMP_Text ageText;

    [Header("Sizing")]
    public float rowMinHeight = 40f;
    public float spacing = 12f;
    public float nameMinWidth = 300f;   // flexible (fills remaining)
    public float posWidth = 60f;        // fixed
    public float ovrWidth = 48f;        // fixed
    public float ageWidth = 48f;        // fixed

    void Reset()  { AutoWire(); Apply(); }
    void Awake()  { AutoWire(); Apply(); }

    public void AutoWire()
    {
        nameText ??= FindText("NameText");
        posText  ??= FindText("PosText");
        ovrText  ??= FindText("OvrText");
        ageText  ??= FindText("AgeText");
    }

    public void Apply()
    {
        var h = GetComponent<HorizontalLayoutGroup>() ?? gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        h.padding = new RectOffset(12, 12, 6, 6);

        var le = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
        le.minHeight = rowMinHeight;

        Lock(nameText, nameMinWidth, flexible:true, align: TextAlignmentOptions.MidlineLeft);
        Lock(posText,  posWidth,  align: TextAlignmentOptions.Midline);
        Lock(ovrText,  ovrWidth,  align: TextAlignmentOptions.Midline);
        Lock(ageText,  ageWidth,  align: TextAlignmentOptions.Midline);
    }

    void Lock(TMP_Text txt, float width, bool flexible = false, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft)
    {
        if (!txt) return;
        var le = txt.GetComponent<LayoutElement>() ?? txt.gameObject.AddComponent<LayoutElement>();
        if (flexible)
        {
            le.minWidth = width;
            le.preferredWidth = -1;
            le.flexibleWidth = 1f;
        }
        else
        {
            le.minWidth = width;
            le.preferredWidth = width;
            le.flexibleWidth = 0f;
        }
        txt.enableAutoSizing = false;
        txt.alignment = align;
    }

    TMP_Text FindText(string child)
    {
        var t = transform.Find(child);
        return t ? t.GetComponent<TMP_Text>() : null;
    }
}
