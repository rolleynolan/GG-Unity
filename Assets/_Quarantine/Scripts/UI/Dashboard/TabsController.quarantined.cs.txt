using UnityEngine;
using UnityEngine.UI;

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

        if (toggleGroup == null) toggleGroup = GetComponent<ToggleGroup>();
        if (toggleGroup != null) toggleGroup.allowSwitchOff = false;

        if (defaultActiveIndex >= 0) this.defaultActiveIndex = defaultActiveIndex;

        for (int i = 0; i < _tabs.Length; i++)
        {
            var t = _tabs[i];
            var idx = i;
            t.group = toggleGroup;
            t.onValueChanged.RemoveAllListeners();
            t.onValueChanged.AddListener(on =>
            {
                if (on) Activate(idx);
                else if (!AnyOn()) t.isOn = true; // don't allow all-off
            });
        }

        // ensure exactly one is on
        if (!AnyOn())
        {
            var idx = Mathf.Clamp(this.defaultActiveIndex, 0, _tabs.Length - 1);
            _tabs[idx].isOn = true;
            Activate(idx);
        }
        else
        {
            // sync panels to whatever is currently on
            for (int i = 0; i < _tabs.Length; i++)
                if (_tabs[i].isOn) { Activate(i); break; }
        }
    }

    private void OnEnable()
    {
        if (_tabs == null || _panels == null) return;
        if (!AnyOn() && _tabs.Length > 0)
        {
            var idx = Mathf.Clamp(defaultActiveIndex, 0, _tabs.Length - 1);
            _tabs[idx].isOn = true;
            Activate(idx);
        }
    }

    private void Activate(int index)
    {
        if (_panels == null) return;
        for (int i = 0; i < _panels.Length; i++)
            if (_panels[i] != null) _panels[i].SetActive(i == index);
    }

    private bool AnyOn()
    {
        if (_tabs == null) return false;
        foreach (var t in _tabs) if (t != null && t.isOn) return true;
        return false;
    }
}
