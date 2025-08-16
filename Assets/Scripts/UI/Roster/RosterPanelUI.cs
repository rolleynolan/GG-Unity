using TMPro;
using UnityEngine;

namespace GG.UI.Roster
{
    public class RosterPanelUI : MonoBehaviour
    {
        public TMP_Text content;
        public string TeamAbbr = "ATL";

        void OnEnable()
        {
            if (!content)
            {
                var go = new GameObject("RosterText", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                content = go.AddComponent<TMP_Text>();
            }
            // For now: simple placeholder. Later: read /data/league_state.json and list players.
            content.text = $"Roster for {TeamAbbr}\n(Connect to league_state.json to show players)";
        }
    }
}
