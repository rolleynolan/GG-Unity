using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using GridironGM.Data;

namespace GridironGM.UI
{
    public class TeamRowUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image logoImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text conferenceText;

        private string teamAbbreviation;
        private Action OnRowClicked;

        // Existing method kept for backward compatibility
        public void SetData(TeamDataUI data)
        {
            if (data == null)
            {
                Debug.LogWarning("TeamRowUI.SetData called with null data!");
                return;
            }

            if (logoImage != null)
            {
                logoImage.sprite = data.logo;
            }

            if (nameText != null)
            {
                nameText.text = data.teamName;
            }

            if (conferenceText != null)
            {
                conferenceText.text = data.teamConference;
            }

            teamAbbreviation = data.abbreviation;
        }

        // New API for setting data from TeamData model
        public void Set(TeamData data, Action onClick)
        {
            if (data == null) return;

            if (nameText != null)
            {
                nameText.text = $"{data.city} {data.name}";
            }

            if (conferenceText != null)
            {
                conferenceText.text = data.conference;
            }

            if (logoImage != null)
            {
                Sprite sprite = Resources.Load<Sprite>($"TeamSprites/{data.abbreviation}");
                if (sprite == null)
                {
                    Debug.LogWarning($"Missing logo sprite for {data.abbreviation}");
                }
                logoImage.sprite = sprite;
            }

            teamAbbreviation = data.abbreviation;
            OnRowClicked = onClick;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"TeamRowUI clicked: {teamAbbreviation}");
            OnRowClicked?.Invoke();
        }
    }
}
