using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RowVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Color baseColor     = new Color(0.10f, 0.18f, 0.28f, 1f);
    public Color altColor      = new Color(0.12f, 0.20f, 0.32f, 1f);
    public Color hoverColor    = new Color(0.18f, 0.28f, 0.40f, 1f);
    public Color selectedColor = new Color(0.26f, 0.36f, 0.50f, 1f);

    [HideInInspector] public int rowIndex;
    [HideInInspector] public System.Action<RowVisual> onSelected;

    Image _bg;
    bool _hover;
    bool _selected;

    void Awake() { _bg = GetComponent<Image>(); Apply(); }

    public void Initialize(int index) { rowIndex = index; _selected = false; _hover = false; Apply(); }
    public void SetSelected(bool v) { _selected = v; Apply(); }

    void Apply()
    {
        if (!_bg) return;
        if (_selected) { _bg.color = selectedColor; return; }
        if (_hover)    { _bg.color = hoverColor;    return; }
        _bg.color = (rowIndex % 2 == 0) ? baseColor : altColor;
    }

    public void OnPointerEnter(PointerEventData eventData) { _hover = true; Apply(); }
    public void OnPointerExit(PointerEventData eventData)  { _hover = false; Apply(); }
    public void OnPointerClick(PointerEventData eventData) { onSelected?.Invoke(this); }
}
