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
    [SerializeField] bool pinHeader = true;      // pinned (not scrolling). Set false for scrolling header.
    [SerializeField] bool showHeader = true;

    Image _selectedBG;
    RectTransform _pinnedHeaderRT;   // created at runtime under the ScrollRect viewport
    float _headerHeightCached;

    void OnRectTransformDimensionsChange()
    {
        ReapplyRowWidths();
        if (pinHeader && _pinnedHeaderRT) ApplyHeaderLayout(_pinnedHeaderRT.gameObject);
    }

    public void ShowRosterForTeam(string abbr)
    {
        if (!EnsureWired())
        {
            Debug.LogWarning("[RosterPanel] Missing Content or PlayerRowUI prefab refs.");
            Debug.Log($"[RosterPanel] ShowRosterForTeam({abbr})\n{System.Environment.StackTrace}");
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
        EnsureHeader();

        for (int i = 0; i < team.players.Count; i++)
        {
            var p   = team.players[i];
            var row = Instantiate(playerRowPrefab, content);

            // Background + zebra color
            var bg = row.GetComponent<Image>() ?? row.AddComponent<Image>();
            var zebraIndex = showHeader && !pinHeader ? i + 1 : i; // if header scrolls, offset
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

    void EnsureHeader()
    {
        if (!showHeader) return;

        if (pinHeader)
        {
            // Build or reuse pinned header under viewport (not in content)
            var sr = GetComponentInChildren<ScrollRect>(true);
            if (!sr || !sr.viewport) return;

            if (!_pinnedHeaderRT)
            {
                var header = Instantiate(playerRowPrefab, sr.viewport);
                header.name = "PinnedHeader";
                _pinnedHeaderRT = (RectTransform)header.transform;

                // Non-interactive / no binder
                var btn = header.GetComponent<Button>(); if (btn) Destroy(btn);
                var binder = header.GetComponent<PlayerRowBinder>(); if (binder) Destroy(binder);

                // Background
                var bg = header.GetComponent<Image>() ?? header.AddComponent<Image>();
                bg.color = headerBgColor;

                // Anchor to top-stretch
                _pinnedHeaderRT.anchorMin = new Vector2(0, 1);
                _pinnedHeaderRT.anchorMax = new Vector2(1, 1);
                _pinnedHeaderRT.pivot     = new Vector2(0.5f, 1f);

                // Label
                ApplyRowLayout(header);
                SetHeaderLabels(header);
            }

            ApplyHeaderLayout(_pinnedHeaderRT.gameObject);
            EnsureHeaderSpacer();
        }
        else
        {
            // Scrolling header: inject as first row into content
            var row = Instantiate(playerRowPrefab, content);
            row.name = "HeaderRow";
            var btn = row.GetComponent<Button>(); if (btn) Destroy(btn);
            var binder = row.GetComponent<PlayerRowBinder>(); if (binder) Destroy(binder);
            ApplyRowLayout(row);
            var bg = row.GetComponent<Image>() ?? row.AddComponent<Image>();
            bg.color = headerBgColor;
            SetHeaderLabels(row);
        }
    }

    void SetHeaderLabels(GameObject row)
    {
        var name = FindTMP(row.transform, "NameText");
        var pos  = FindTMP(row.transform, "PosText");
        var ovr  = FindTMP(row.transform, "OvrText");
        var age  = FindTMP(row.transform, "AgeText");

        if (name) { name.text = "NAME"; name.fontStyle = FontStyles.Bold; name.color = headerTextTint; }
        if (pos)  { pos.text  = "POS";  pos.fontStyle  = FontStyles.Bold; pos.color  = headerTextTint; }
        if (ovr)  { ovr.text  = "OVR";  ovr.fontStyle  = FontStyles.Bold; ovr.color  = headerTextTint; }
        if (age)  { age.text  = "AGE";  age.fontStyle  = FontStyles.Bold; age.color  = headerTextTint; }
    }

    void ApplyHeaderLayout(GameObject header)
    {
        var rt = (RectTransform)header.transform;
        float h = Mathf.Max(rowMinHeight + padY * 2, 40f);
        _headerHeightCached = h;
        rt.offsetMin = new Vector2(0, -h);
        rt.offsetMax = new Vector2(0, 0);

        // Ensure columns align like rows
        ApplyRowLayout(header);
    }

    void EnsureHeaderSpacer()
    {
        if (!content) return;
        // Insert a spacer as first child so rows start below the pinned header
        if (content.childCount == 0 || content.GetChild(0).name != "HeaderSpacer")
        {
            var spacer = new GameObject("HeaderSpacer", typeof(RectTransform), typeof(LayoutElement));
            var rt = (RectTransform)spacer.transform;
            rt.SetParent(content, false);
            rt.SetSiblingIndex(0);
            var le = spacer.GetComponent<LayoutElement>();
            le.minHeight = _headerHeightCached;
            le.preferredHeight = _headerHeightCached;
        }
        else
        {
            var le = content.GetChild(0).GetComponent<LayoutElement>();
            if (le) { le.minHeight = _headerHeightCached; le.preferredHeight = _headerHeightCached; }
        }
    }

    // ---------------- helpers ----------------

    void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
        _selectedBG = null;
        // Keep pinned header if created
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

        var name = FindTMP(row.transform, "NameText");
        var pos  = FindTMP(row.transform, "PosText");
        var ovr  = FindTMP(row.transform, "OvrText");
        var age  = FindTMP(row.transform, "AgeText");

        LockFixed(pos, posWidth);
        LockFixed(ovr, ovrWidth);
        LockFixed(age, ageWidth);

        ConfigureText(name, TextAlignmentOptions.MidlineLeft);
        ConfigureText(pos,  TextAlignmentOptions.Midline);
        ConfigureText(ovr,  TextAlignmentOptions.Midline);
        ConfigureText(age,  TextAlignmentOptions.Midline);

        var parentRT = (RectTransform)row.transform.parent;
        float parentW = parentRT.rect.width;
        float totalFixed = posWidth + ovrWidth + ageWidth + spacing * 3 + padX * 2;
        float nameW = Mathf.Max(nameMinWidth, parentW - totalFixed);
        LockExact(name, nameW);
    }

    void ReapplyRowWidths()
    {
        if (!content) return;
        for (int i = 0; i < content.childCount; i++)
        {
            var row = content.GetChild(i).gameObject;
            if (row.name == "HeaderSpacer") continue;
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
        txt.textWrappingMode  = TextWrappingModes.NoWrap;
        txt.overflowMode      = TextOverflowModes.Ellipsis;
    }

    static void LockExact(TMP_Text txt, float width)
    {
        if (!txt) return;
        var le = txt.GetComponent<LayoutElement>() ?? txt.gameObject.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.flexibleWidth = 0f;

        txt.enableAutoSizing  = false;
        txt.textWrappingMode  = TextWrappingModes.NoWrap;
        txt.overflowMode      = TextOverflowModes.Ellipsis;
    }

    static void ConfigureText(TMP_Text txt, TextAlignmentOptions align)
    {
        if (!txt) return;
        txt.alignment        = align;
        txt.textWrappingMode = TextWrappingModes.NoWrap;
        txt.overflowMode     = TextOverflowModes.Ellipsis;
    }
}
