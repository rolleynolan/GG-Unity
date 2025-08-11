using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GridironGM.Data;

public class TeamRowUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Button button;                // Button on row root
    [SerializeField] private Image background;             // Background Image
    [SerializeField] private TMP_Text label;               // Optional label
    [SerializeField] private GameObject selectedHighlight; // Optional child (border/check)

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.14f, 0.14f, 0.14f, 1f);   // #242424
    [SerializeField] private Color selectedColor = new Color(0.22f, 0.32f, 0.52f, 1f); // #385284

    public TeamData Team { get; private set; }
    public System.Action<TeamRowUI> OnClicked;

    public void Init(TeamData team, System.Action<TeamRowUI> onClicked)
    {
        Team = team;
        OnClicked = onClicked;

        if (label != null)
            label.text = $"{team.city} {team.name} ({team.abbreviation})";

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClicked?.Invoke(this));
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (background != null)
            background.color = isSelected ? selectedColor : normalColor;

        if (selectedHighlight != null)
            selectedHighlight.SetActive(isSelected);
    }
}
