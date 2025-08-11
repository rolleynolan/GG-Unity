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
            if (nameText)       nameText.text = $"{data.city} {data.name}";
            if (conferenceText) conferenceText.text = data.conference;

            var sprite = Resources.Load<Sprite>($"TeamSprites/{abbr}") ?? Resources.Load<Sprite>("TeamSprites/Generic");
            if (logoImage)
            {
                logoImage.sprite  = sprite;
                logoImage.enabled = sprite != null;
            }

            // Robust button hookup
            var btn = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
            if (!btn)
            {
                Debug.LogError("[TeamRowUI] Button missing on prefab. Add Image (Raycast ON) + Button on root.");
                return;
            }

            btn.onClick.RemoveAllListeners();
            if (onClick != null) btn.onClick.AddListener(() => onClick());
        }
    }
}
