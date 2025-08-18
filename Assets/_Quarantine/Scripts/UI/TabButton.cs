using UnityEngine;
using UnityEngine.UI;

namespace GridironGM.UI
{
    [RequireComponent(typeof(Button))]
    public class TabButton : MonoBehaviour
    {
        [SerializeField] private GameObject targetPage;
        private ScreenManager manager;

        private void Awake()
        {
            manager = GetComponentInParent<ScreenManager>();
            if (manager == null)
                Debug.LogError("[TabButton] ScreenManager not found in parent hierarchy.", this);

            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            if (GetComponent<Toggle>() != null)
                Debug.LogWarning("[TabButton] Tab should not have a Toggle component.", this);
        }

        public void OnClick()
        {
            if (targetPage == null)
            {
                Debug.LogError("[TabButton] Target page not assigned.", this);
                return;
            }

            if (manager == null)
            {
                Debug.LogError("[TabButton] ScreenManager reference missing.", this);
                return;
            }

            manager.Show(targetPage);
        }

        private void OnValidate()
        {
            if (targetPage == null)
                Debug.LogWarning("[TabButton] targetPage is not set.", this);
        }
    }
}

