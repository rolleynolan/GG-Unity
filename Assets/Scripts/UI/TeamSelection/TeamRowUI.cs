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

        public void Set(GridironGM.Data.TeamData data, System.Action onClick)
        {
            abbr = data.abbreviation;
            if (nameText)      nameText.text      = $"{data.city} {data.name}";
            if (conferenceText) conferenceText.text = data.conference;

            var sprite = Resources.Load<Sprite>($"TeamSprites/{abbr}");
            if (!sprite)
            {
                Debug.LogWarning($"[TeamRowUI] Missing sprite for {abbr}. Using fallback if available.");
                sprite = Resources.Load<Sprite>("TeamSprites/Generic");
            }
            if (logoImage)
            {
                logoImage.sprite  = sprite;
                logoImage.enabled = sprite != null;
            }

            var btn = GetComponent<UnityEngine.UI.Button>();
            if (btn != null && onClick != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onClick());
            }
        }
    }
}
