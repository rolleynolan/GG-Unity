using UnityEngine;
using UnityEngine.UI;

namespace GridironGM.UI
{
    /// <summary>
    /// Optional helper placed on a Tabs root to auto-wire child TabButtons.
    /// </summary>
    public class TabButtonBinder : MonoBehaviour
    {
        private void Awake()
        {
            var tabs = GetComponentsInChildren<TabButton>(true);
            foreach (var tab in tabs)
            {
                var btn = tab.GetComponent<Button>();
                if (btn == null) continue;
                btn.onClick.RemoveListener(tab.OnClick);
                btn.onClick.AddListener(tab.OnClick);
            }
            Debug.Log($"[TabButtonBinder] Wired {tabs.Length} tabs under {name}.");
        }
    }
}

