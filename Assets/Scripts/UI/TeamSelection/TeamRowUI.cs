using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GridironGM.Data;

public class TeamRowUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image logoImage;            // <— assign to the "Logo" Image

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.14f, 0.14f, 0.14f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.22f, 0.32f, 0.52f, 1f);

    public TeamData Team { get; private set; }
    public System.Action<TeamRowUI> OnClicked;

    private void Awake()
    {
        // Light auto-wire: find child named "Logo" if not set
        if (button == null) button = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
        if (background == null) background = GetComponent<Image>();
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (logoImage == null)
        {
            foreach (var img in GetComponentsInChildren<Image>(true))
            {
                if (img.name.Equals("Logo", System.StringComparison.OrdinalIgnoreCase))
                { logoImage = img; break; }
            }
        }
        if (logoImage != null) logoImage.raycastTarget = false;
    }

    public void Init(TeamData team, System.Action<TeamRowUI> onClicked)
    {
        Team = team;
        OnClicked = onClicked;

        if (label != null)
            label.text = $"{team.city} {team.name} ({team.abbreviation})";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClicked?.Invoke(this));
        }

        // === SIMPLE LOGO LOAD BY ABBR FROM Resources/TeamSprites ===
        var key = (team.abbreviation ?? "").Trim().ToUpperInvariant();
        var sprite = Resources.Load<Sprite>($"TeamSprites/{key}");
        if (logoImage != null)
        {
            logoImage.sprite = sprite;
            logoImage.enabled = sprite != null;
            logoImage.preserveAspect = true;
#if UNITY_EDITOR
            if (sprite == null) Debug.LogWarning($"[TeamRowUI] Missing logo sprite at Resources/TeamSprites/{key}.png");
#endif
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (background != null) background.color = isSelected ? selectedColor : normalColor;
    }
}
