using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class TabController : MonoBehaviour
{
    [Serializable]
    public class Tab
    {
        public string id;                 // e.g., "Roster", "Schedule", "Standings"
        public Button button;             // The tab button
        public GameObject panel;          // Root panel to show/hide
        public Selectable firstSelected;  // Optional: focus target when tab opens
        public UnityEvent onShown;        // Optional extra hook for inspector
    }

    [Header("Tabs (order matters)")]
    [SerializeField] private Tab[] tabs;

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

        // Validate and hook up buttons
        for (int i = 0; i < tabs.Length; i++)
        {
            int capture = i;
            if (tabs[i].button == null || tabs[i].panel == null)
            {
                Debug.LogError($"[TabController] Tab {i} missing button or panel.", this);
                continue;
            }
            tabs[i].button.onClick.RemoveAllListeners();
            tabs[i].button.onClick.AddListener(() => ShowTab(capture));
        }

        // Hide all initially to avoid flicker
        for (int i = 0; i < tabs.Length; i++)
            SafeSetActive(tabs[i].panel, false);

        prefsKey = $"TabController.LastTab.{SceneManager.GetActiveScene().name}.{gameObject.name}";

        int startIndex = defaultIndex;
        if (rememberLastTab && PlayerPrefs.HasKey(prefsKey))
        {
            string lastId = PlayerPrefs.GetString(prefsKey, tabs[Mathf.Clamp(defaultIndex,0,tabs.Length-1)].id);
            int found = IndexOfTab(lastId);
            if (found >= 0) startIndex = found;
        }

        ShowTab(Mathf.Clamp(startIndex, 0, tabs.Length - 1));
    }

    public void ShowTab(string id)
    {
        int idx = IndexOfTab(id);
        if (idx < 0)
        {
            Debug.LogWarning($"[TabController] Unknown tab id '{id}'.");
            return;
        }
        ShowTab(idx);
    }

    public void ShowTab(int index)
    {
        if (tabs == null || tabs.Length == 0) return;
        index = Mathf.Clamp(index, 0, tabs.Length - 1);
        if (activeIndex == index) return;

        // Hide previous
        if (activeIndex >= 0 && activeIndex < tabs.Length)
            SafeSetActive(tabs[activeIndex].panel, false);

        // Show new
        activeIndex = index;
        var t = tabs[activeIndex];
        SafeSetActive(t.panel, true);

        // Layout fix: force rebuild so content doesn't appear blank
        var rt = t.panel.GetComponent<RectTransform>();
        if (rt != null)
        {
            Canvas.ForceUpdateCanvases();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();
        }

        // Focus (optional)
        if (t.firstSelected != null)
            EventSystem.current?.SetSelectedGameObject(t.firstSelected.gameObject);

        // ITabView callback
        var view = t.panel.GetComponentInChildren<ITabView>(includeInactive: false);
        view?.OnTabShown();

        // UnityEvent hook (inspector)
        t.onShown?.Invoke();

        // Persist
        if (rememberLastTab)
            PlayerPrefs.SetString(prefsKey, t.id);

        Debug.Log($"[TabController] Shown tab: {t.id} (index {activeIndex})");
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
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}
