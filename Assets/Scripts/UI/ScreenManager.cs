using System.Collections.Generic;
using UnityEngine;

namespace GridironGM.UI
{
    /// <summary>
    /// Controls visibility of page panels. Exactly one page is active at a time.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        [Tooltip("Pages managed by this screen manager.")]
        public List<GameObject> pages = new List<GameObject>();

        [Tooltip("Page displayed on start.")]
        public GameObject defaultPage;

        private void Awake()
        {
            // Auto-discover pages if list is empty
            if (pages == null || pages.Count == 0)
            {
                Transform pagesRoot = transform.Find("Pages");
                if (pagesRoot != null)
                {
                    pages = new List<GameObject>();
                    foreach (Transform child in pagesRoot)
                        pages.Add(child.gameObject);
                }
                else
                {
                    Debug.LogWarning("[ScreenManager] 'Pages' container not found.", this);
                }
            }

            if (pages == null)
                pages = new List<GameObject>();

            foreach (var p in pages)
                if (p) p.SetActive(false);

            if (defaultPage == null && pages.Count > 0)
                defaultPage = pages[0];

            if (defaultPage != null && !pages.Contains(defaultPage))
                pages.Add(defaultPage);

            if (defaultPage != null)
            {
                defaultPage.SetActive(true);
                Debug.Log($"[ScreenManager] Default page active: {defaultPage.name}");
            }
            else
            {
                Debug.LogWarning("[ScreenManager] No default page set.", this);
            }

            Debug.Log($"[ScreenManager] Registered {pages.Count} pages.");
        }

        /// <summary>
        /// Show the requested page. Falls back to default page on invalid input.
        /// </summary>
        public void Show(GameObject page)
        {
            if (page == null)
            {
                Debug.LogError("[ScreenManager] Show called with null page.", this);
                page = defaultPage;
            }

            if (!pages.Contains(page))
            {
                Debug.LogWarning($"[ScreenManager] Page '{page?.name}' not registered. Using default page.", this);
                page = defaultPage;
            }

            foreach (var p in pages)
            {
                if (p == null) continue;
                bool active = p == page;
                if (p.activeSelf != active)
                    p.SetActive(active);
            }

            if (page != null)
                Debug.Log($"[ScreenManager] Showing page: {page.name}");
        }
    }
}

