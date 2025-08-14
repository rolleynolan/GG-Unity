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
    [SerializeField] float rowMinHeight = 40f;
    [SerializeField] float spacing = 12f;
    [SerializeField] float nameMinWidth = 300f;  // flexible column
    [SerializeField] float posWidth     = 60f;   // fixed
    [SerializeField] float ovrWidth     = 48f;   // fixed
    [SerializeField] float ageWidth     = 48f;   // fixed

    [Header("Colors")]
    [SerializeField] Color baseColor     = new Color(0.10f, 0.18f, 0.28f, 1f);
    [SerializeField] Color altColor      = new Color(0.12f, 0.20f, 0.32f, 1f);
    [SerializeField] Color selectedColor = new Color(0.26f, 0.36f, 0.50f, 1f);

    Image _selectedBG;

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

        for (int i = 0; i < team.players.Count; i++)
        {
            var p   = team.players[i];
            var row = Instantiate(playerRowPrefab, content);

            // Ensure background + zebra color
            var bg = row.GetComponent<Image>();
            if (!bg) bg = row.AddComponent<Image>();
            bg.color = (i % 2 == 0) ? baseColor : altColor;

            // Make the row clickable for selection
            var btn = row.GetComponent<Button>() ?? row.AddComponent<Button>();
            btn.transition = Selectable.Transition.None; // color change handled by us
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (_selectedBG) _selectedBG.color = (_selectedBG.transform.GetSiblingIndex() % 2 == 0) ? baseColor : altColor;
                _selectedBG = bg;
                _selectedBG.color = selectedColor;
            });

            // Enforce fixed column layout before binding text
            EnsureRowLayout(row);

            // Bind data
            var binder = row.GetComponent<PlayerRowBinder>() ?? row.AddComponent<PlayerRowBinder>();
            binder.Bind(p);
        }

        Debug.Log($"[RosterPanel] Rendered {team.players.Count} players for {abbr}");
    }

    // ----- helpers -----

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

    void EnsureRowLayout(GameObject row)
    {
        // Row layout container
        var h = row.GetComponent<HorizontalLayoutGroup>() ?? row.AddComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        h.padding = new RectOffset(12, 12, 6, 6);

        var leRow = row.GetComponent<LayoutElement>() ?? row.AddComponent<LayoutElement>();
        leRow.minHeight = rowMinHeight;

        // Lock columns by name; create LayoutElements if missing
        LockColumn(FindTMP(row.transform, "NameText"), nameMinWidth, flexible: true,  TextAlignmentOptions.MidlineLeft);
        LockColumn(FindTMP(row.transform, "PosText"),  posWidth,     flexible: false, TextAlignmentOptions.Midline);
        LockColumn(FindTMP(row.transform, "OvrText"),  ovrWidth,     flexible: false, TextAlignmentOptions.Midline);
        LockColumn(FindTMP(row.transform, "AgeText"),  ageWidth,     flexible: false, TextAlignmentOptions.Midline);
    }

    static TMP_Text FindTMP(Transform root, string childName)
    {
        var t = root.Find(childName);
        return t ? t.GetComponent<TMP_Text>() : null;
    }

    static void LockColumn(TMP_Text txt, float width, bool flexible, TextAlignmentOptions align)
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
}

