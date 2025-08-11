using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GridironGM.Data;

public class TeamRowUI : MonoBehaviour
{
    [Header("Wiring (optional)")]
    [SerializeField] private Button button;                // Will auto-find if null
    [SerializeField] private Image background;             // Will auto-find if null
    [SerializeField] private TMP_Text label;               // Will auto-find if null
    [SerializeField] private GameObject selectedHighlight; // Optional; we disable raycasts

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.14f, 0.14f, 0.14f, 1f);   // #242424
    [SerializeField] private Color selectedColor = new Color(0.22f, 0.32f, 0.52f, 1f); // #385284

    public TeamData Team { get; private set; }
    public System.Action<TeamRowUI> OnClicked;

    private void Awake() => AutoWireIfNeeded();
#if UNITY_EDITOR
    private void OnValidate() => AutoWireIfNeeded();
#endif

    private void AutoWireIfNeeded()
    {
        if (button == null)
            button = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);

        if (background == null)
        {
            // Prefer an Image named "Background", else use the first Image on this GO
            var images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject == gameObject || img.name.ToLower().Contains("background"))
                { background = img; break; }
            }
            if (background == null && images.Length > 0) background = images[0];
        }

        if (label == null)
            label = GetComponentInChildren<TMP_Text>(true);

        // Make sure highlight never blocks clicks
        if (selectedHighlight != null)
        {
            var imgs = selectedHighlight.GetComponentsInChildren<Image>(true);
            foreach (var img in imgs) img.raycastTarget = false;
        }
    }

    public void Init(TeamData team, System.Action<TeamRowUI> onClicked)
    {
        AutoWireIfNeeded();

        Team = team;
        OnClicked = onClicked;

        if (label != null) label.text = $"{team.city} {team.name} ({team.abbreviation})";
        else Debug.LogWarning($"[TeamRowUI] No TMP_Text found on '{name}'");

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClicked?.Invoke(this));
        }
        else
        {
            Debug.LogError($"[TeamRowUI] No Button found on '{name}' — row will not be clickable.");
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (background != null) background.color = isSelected ? selectedColor : normalColor;
        if (selectedHighlight != null) selectedHighlight.SetActive(isSelected);
    }
}

