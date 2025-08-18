using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class TabController : MonoBehaviour
{
    [Serializable]
    public class Tab
    {
        public string id;                 // e.g., "Roster", "Depth", "Schedule"
        public Button button;             // Use Button OR Toggle
        public Toggle toggle;             // Use Toggle OR Button
        public GameObject panel;          // Root to activate/deactivate
        public Selectable firstSelected;  // Optional focus target
        public UnityEvent onShown;        // Optional hook
    }

    [Header("Tabs (order matters)")]
    [SerializeField] private Tab[] tabs = Array.Empty<Tab>();

    [Header("Behavior")]
    [SerializeField] private int defaultIndex = 0;
    [SerializeField] private bool rememberLastTab = true;

    private int activeIndex = -1;
    private string prefsKey;

    void Awake()
    {
        if (tabs == null || tabs.Length == 0)
        {
            Debug.LogError("[TabController] No tabs configured.", this);
            return;
        }

        // Hook listeners and validate
        for (int i = 0; i < tabs.Length; i++)
        {
            int idx = i;
            var t = tabs[i];
            if (t.panel == null)
                Debug.LogError($"[TabController] Tab '{t.id}' missing panel.", this);

            if (t.button != null) t.button.onClick.RemoveAllListeners();
            if (t.toggle != null) t.toggle.onValueChanged.RemoveAllListeners();

            if (t.button != null)
            {
                t.button.onClick.AddListener(() => ShowTab(idx));
            }
            else if (t.toggle != null)
            {
                t.toggle.onValueChanged.AddListener(on => { if (on) ShowTab(idx); });
            }
            else
            {
                Debug.LogError($"[TabController] Tab '{t.id}' needs a Button or Toggle.", this);
            }
        }

        // Hide all to avoid flicker
        for (int i = 0; i < tabs.Length; i++)
            SafeSetActive(tabs[i].panel, false);

        prefsKey = $"TabController.LastTab.{SceneManager.GetActiveScene().name}.{gameObject.name}";

        int start = Mathf.Clamp(defaultIndex, 0, tabs.Length - 1);
        if (rememberLastTab && PlayerPrefs.HasKey(prefsKey))
        {
            string lastId = PlayerPrefs.GetString(prefsKey, tabs[start].id);
            int found = IndexOfTab(lastId);
            if (found >= 0) start = found;
        }

        ShowTab(start);
    }

    public void ShowTab(string id)
    {
        int idx = IndexOfTab(id);
        if (idx >= 0) ShowTab(idx);
        else Debug.LogWarning($"[TabController] Unknown tab id '{id}'.");
    }

    public void ShowTab(int index)
    {
        if (tabs == null || tabs.Length == 0) return;
        index = Mathf.Clamp(index, 0, tabs.Length - 1);
        if (activeIndex == index) return;

        // Deactivate previous
        if (activeIndex >= 0 && activeIndex < tabs.Length)
        {
            var prev = tabs[activeIndex];
            SafeSetActive(prev.panel, false);
            if (prev.toggle) prev.toggle.SetIsOnWithoutNotify(false);
        }

        // Activate current
        activeIndex = index;
        var cur = tabs[activeIndex];

        if (cur.toggle) cur.toggle.SetIsOnWithoutNotify(true);
        SafeSetActive(cur.panel, true);

        // Layout fix on show
        var rt = cur.panel ? cur.panel.GetComponent<RectTransform>() : null;
        if (rt)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();
        }

        // Optional focus
        if (cur.firstSelected != null)
            EventSystem.current?.SetSelectedGameObject(cur.firstSelected.gameObject);

        // ITabView callback
        var view = cur.panel ? cur.panel.GetComponentInChildren<ITabView>(includeInactive: false) : null;
        view?.OnTabShown();

        // Inspector hook
        cur.onShown?.Invoke();

        if (rememberLastTab)
            PlayerPrefs.SetString(prefsKey, cur.id);

        Debug.Log($"[TabController] Shown tab: {cur.id} (index {activeIndex})");
    }

    private int IndexOfTab(string id)
    {
        for (int i = 0; i < tabs.Length; i++)
            if (string.Equals(tabs[i].id, id, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private static void SafeSetActive(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }
}
