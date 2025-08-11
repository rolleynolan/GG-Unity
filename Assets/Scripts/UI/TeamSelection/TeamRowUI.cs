using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace GridironGM.UI.TeamSelection
{
    public class TeamRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text conferenceText;
        [SerializeField] private Image logoImage;

        private string abbr;
        private Button _button;
        private void Awake()
        {
            // Auto-wire if not set
            if (!nameText) nameText = transform.Find("NameText")?.GetComponent<TMP_Text>();
            if (!conferenceText) conferenceText = transform.Find("ConferenceText")?.GetComponent<TMP_Text>();
            if (!logoImage) logoImage = transform.Find("LogoImage")?.GetComponent<Image>();

            // Ensure button is present
            _button = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
            if (_button == null)
            {
                Debug.LogError("[TeamRowUI] Button component missing on prefab/root. Add a Button + Image with Raycast Target ON.");
            }
        }


        public void Set(GridironGM.Data.TeamData data, System.Action onClick)
        {
            abbr = data.abbreviation;
            if (nameText) nameText.text = $"{data.city} {data.name}";
            if (conferenceText) conferenceText.text = data.conference;

            var sprite = Resources.Load<Sprite>($"TeamSprites/{abbr}");
            if (!sprite)
            {
                Debug.LogWarning($"[TeamRowUI] Missing sprite for {abbr}. Using fallback if available.");
                sprite = Resources.Load<Sprite>("TeamSprites/Generic");
            }
            if (logoImage)
            {
                logoImage.sprite = sprite;
                logoImage.enabled = sprite != null;
            }

            // --- CLICK HOOKUP (robust) ---
            var btn = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
            if (btn == null)
            {
                Debug.LogError("[TeamRowUI] Button component missing on prefab/root. Add a Button + Image with Raycast Target ON.");
                return;
            }

            if (onClick == null)
            {
                Debug.LogError("[TeamRowUI] onClick callback is null. TeamSelectionUI must pass a handler.");
                btn.onClick.RemoveAllListeners(); // prevent stale listeners
                return;
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onClick());
            // Optional: quick proof in Console
            // Debug.Log($"[TeamRowUI] Hooked click for {abbr}");
        }
        }
    }
