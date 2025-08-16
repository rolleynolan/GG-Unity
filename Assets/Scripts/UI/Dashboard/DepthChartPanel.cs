using UnityEngine;
using TMPro;

namespace GridironGM.UI.Dashboard
{
    public class DepthChartPanel : MonoBehaviour
    {
        [SerializeField] private Transform content; // vertical layout container
        [SerializeField] private TMP_Text placeholder; // text element to show stub message

        private string teamAbbr;

        public void SetTeam(string abbr)
        {
            teamAbbr = abbr;

            if (content != null)
            {
                for (int i = content.childCount - 1; i >= 0; i--)
                    Destroy(content.GetChild(i).gameObject);
            }

            if (placeholder != null)
            {
                placeholder.gameObject.SetActive(true);
                placeholder.textWrappingMode = TextWrappingModes.NoWrap;
                placeholder.text = $"Depth chart for {abbr} coming soon.";
            }
        }
    }
}

