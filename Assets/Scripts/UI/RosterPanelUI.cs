using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RosterPanelUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] Transform content;              // Scroll View / Viewport / Content
    [SerializeField] GameObject playerRowPrefab;     // PlayerRowUI prefab

    [Header("Row Layout")]
    [SerializeField] float rowMinHeight = 44f;
    [SerializeField] float spacing = 16f;
    [SerializeField] float nameMinWidth = 420f;  // Name column gets remaining width but never below this
    [SerializeField] float posWidth     = 70f;   // fixed
    [SerializeField] float ovrWidth     = 52f;   // fixed
    [SerializeField] float ageWidth     = 52f;   // fixed
    [SerializeField] int   padX         = 12;    // HorizontalLayoutGroup padding
    [SerializeField] int   padY         = 6;

    [Header("Colors")]
    [SerializeField] Color baseColor      = new Color(0.10f, 0.18f, 0.28f, 1f);
    [SerializeField] Color altColor       = new Color(0.12f, 0.20f, 0.32f, 1f);
    [SerializeField] Color selectedColor  = new Color(0.26f, 0.36f, 0.50f, 1f);
    [SerializeField] Color headerBgColor  = new Color(0.08f, 0.14f, 0.22f, 1f);
    [SerializeField] Color headerTextTint = new Color(0.85f, 0.92f, 1f, 0.9f);

    [Header("Header")]
    [SerializeField] bool showHeader = true;     // toggle if you ever want to hide it

    Image _selectedBG;

    void OnRectTransformDimensionsChange()
    {
        ReapplyRowWidths();
    }

    public void ShowRosterForTeam(string abbr)
    {
        if (!EnsureWired())
        {
            Debug.LogWarning("[RosterPanel] Missing Content or PlayerRowUI prefab refs.");
            return;
        }

        var team = RosterService.LoadRosterFor(abbr);
        if (team == null || team.players == null || team.players.Count == 0)
        {
            ClearContent();
            Debug.LogWarning($"[RosterPanel] No roster for {abbr}");
            return;
        }

        ClearContent();

        // ----- HEADER (scrolls with list) -----
        if (showHeader) AddHeaderRow();

        for (int i = 0; i < team.players.Count; i++)
        {
            var p   = team.players[i];
            var row = Instantiate(playerRowPrefab, content);

            // Background + zebra color
            var bg = row.GetComponent<Image>() ?? row.AddComponent<Image>();
            var zebraIndex = showHeader ? i + 1 : i; // keep pattern after header
            bg.color = (zebraIndex % 2 == 0) ? baseColor : altColor;

            // Click to select highlight
            var btn = row.GetComponent<Button>() ?? row.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (_selectedBG)
                    _selectedBG.color = (_selectedBG.transform.GetSiblingIndex() % 2 == 0) ? baseColor : altColor;
                _selectedBG = bg;
                _selectedBG.color = selectedColor;
            });

            // Layout & widths before binding
            ApplyRowLayout(row);

            // Bind data
            var binder = row.GetComponent<PlayerRowBinder>() ?? row.AddComponent<PlayerRowBinder>();
            binder.Bind(p);
        }

        ReapplyRowWidths();
        Debug.Log($"[RosterPanel] Rendered {team.players.Count} players for {abbr}");
    }

    // ---------------- header ----------------

    void AddHeaderRow()
    {
        // Reuse the row prefab, but turn it into a non-interactive header
        var row = Instantiate(playerRowPrefab, content);
        row.name = "HeaderRow";

        // Remove selection behavior
        var btn = row.GetComponent<Button>();
        if (btn) Destroy(btn);

        // Remove binder so it won't overwrite our labels
        var binder = row.GetComponent<PlayerRowBinder>();
        if (binder) Destroy(binder);

        // Layout first, then label
        ApplyRowLayout(row);

        var bg = row.GetComponent<Image>() ?? row.AddComponent<Image>();
        bg.color = headerBgColor;

        var name = FindTMP(row.transform, "NameText");
        var pos  = FindTMP(row.transform, "PosText");
        var ovr  = FindTMP(row.transform, "OvrText");
        var age  = FindTMP(row.transform, "AgeText");

        if (name) { name.text = "NAME"; name.fontStyle = FontStyles.Bold; name.color = headerTextTint; }
        if (pos)  { pos.text  = "POS";  pos.fontStyle  = FontStyles.Bold; pos.color  = headerTextTint; }
        if (ovr)  { ovr.text  = "OVR";  ovr.fontStyle  = FontStyles.Bold; ovr.color  = headerTextTint; }
        if (age)  { age.text  = "AGE";  age.fontStyle  = FontStyles.Bold; age.color  = headerTextTint; }
    }

    // ---------------- helpers ----------------

    void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
        _selectedBG = null;
    }

    bool EnsureWired()
    {
        if (!content)
        {
            var sr = GetComponentInChildren<ScrollRect>(true);
            if (sr && sr.content) content = sr.content;
        }
        if (!playerRowPrefab)
        {
            var res = Resources.Load<GameObject>("UI/PlayerRowUI");
            if (res) playerRowPrefab = res;
        }
        return content && playerRowPrefab;
    }

    void ApplyRowLayout(GameObject row)
    {
        var h = row.GetComponent<HorizontalLayoutGroup>() ?? row.AddComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        h.padding = new RectOffset(padX, padX, padY, padY);

        var leRow = row.GetComponent<LayoutElement>() ?? row.AddComponent<LayoutElement>();
        leRow.minHeight = rowMinHeight;

        // Find TMPs
        var name = FindTMP(row.transform, "NameText");
        var pos  = FindTMP(row.transform, "PosText");
        var ovr  = FindTMP(row.transform, "OvrText");
        var age  = FindTMP(row.transform, "AgeText");

        // Lock fixed columns now
        LockFixed(pos, posWidth);
        LockFixed(ovr, ovrWidth);
        LockFixed(age, ageWidth);

        // Configure text to avoid squish
        ConfigureText(name, TextAlignmentOptions.MidlineLeft);
        ConfigureText(pos,  TextAlignmentOptions.Midline);
        ConfigureText(ovr,  TextAlignmentOptions.Midline);
        ConfigureText(age,  TextAlignmentOptions.Midline);

        // Compute remaining width for Name (exact preferred width)
        var parentRT = (RectTransform)row.transform.parent;
        float parentW = parentRT.rect.width;
        float totalFixed = posWidth + ovrWidth + ageWidth + spacing * 3 + padX * 2;
        float nameW = Mathf.Max(nameMinWidth, parentW - totalFixed);
        LockExact(name, nameW);
    }

    void ReapplyRowWidths()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            var row = content.GetChild(i).gameObject;
            var name = FindTMP(row.transform, "NameText");
            var parentRT = (RectTransform)row.transform.parent;
            float parentW = parentRT.rect.width;
            float totalFixed = posWidth + ovrWidth + ageWidth + spacing * 3 + padX * 2;
            float nameW = Mathf.Max(nameMinWidth, parentW - totalFixed);
            LockExact(name, nameW);
        }
        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)content);
    }

    static TMP_Text FindTMP(Transform root, string childName)
    {
        var t = root.Find(childName);
        return t ? t.GetComponent<TMP_Text>() : null;
    }

    static void LockFixed(TMP_Text txt, float width)
    {
        if (!txt) return;
        var le = txt.GetComponent<LayoutElement>() ?? txt.gameObject.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.flexibleWidth = 0f;

        txt.enableAutoSizing  = false;
        txt.textWrappingMode  = TextWrappingModes.NoWrap;   // TMP 4.x API
        txt.overflowMode      = TextOverflowModes.Ellipsis;
    }

    static void LockExact(TMP_Text txt, float width)
    {
        if (!txt) return;
        var le = txt.GetComponent<LayoutElement>() ?? txt.gameObject.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;   // exact width for Name column
        le.flexibleWidth = 0f;

        txt.enableAutoSizing  = false;
        txt.textWrappingMode  = TextWrappingModes.NoWrap;   // TMP 4.x API
        txt.overflowMode      = TextOverflowModes.Ellipsis;
    }

    static void ConfigureText(TMP_Text txt, TextAlignmentOptions align)
    {
        if (!txt) return;
        txt.alignment        = align;
        txt.textWrappingMode = TextWrappingModes.NoWrap;   // TMP 4.x API
        txt.overflowMode     = TextOverflowModes.Ellipsis;
    }
}
