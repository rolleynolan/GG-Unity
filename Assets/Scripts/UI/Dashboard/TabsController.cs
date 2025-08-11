using UnityEngine;
using UnityEngine.UI;

namespace GridironGM.UI.Dashboard
{
    public class TabsController : MonoBehaviour
    {
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private int defaultActiveIndex = 0;

        private Toggle[] _tabs;
        private GameObject[] _panels;

        public void Configure(Toggle[] tabs, GameObject[] panels, int defaultActiveIndex = -1)
        {
            _tabs = tabs;
            _panels = panels;

            if (defaultActiveIndex >= 0)
                this.defaultActiveIndex = defaultActiveIndex;

            for (int i = 0; i < _tabs.Length; i++)
            {
                var idx = i;
                _tabs[i].group = toggleGroup;
                _tabs[i].onValueChanged.RemoveAllListeners();
                _tabs[i].onValueChanged.AddListener(on => { if (on) Activate(idx); });
            }

            for (int i = 0; i < _tabs.Length; i++)
                _tabs[i].isOn = (i == this.defaultActiveIndex);

            Activate(this.defaultActiveIndex);
        }

        private void Activate(int index)
        {
            if (_panels == null || _tabs == null) return;
            for (int i = 0; i < _panels.Length; i++)
            {
                if (_panels[i] != null)
                    _panels[i].SetActive(i == index);
            }
        }
    }
}

