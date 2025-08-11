using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using GridironGM.Data;

namespace GridironGM.UI
{
    public class RosterPanelUI : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private PlayerRowUI playerRowPrefab;
        [SerializeField] private TMP_Text messageText;

        private RosterByTeam rosters;

        private void Awake()
        {
            rosters = JsonLoader.LoadFromStreamingAssets<RosterByTeam>("rosters_by_team.json");
            if (rosters == null)
            {
                Debug.LogError("rosters_by_team.json missing or invalid");
            }
            else
            {
                Debug.Log($"Loaded rosters for {rosters.Count} teams");
            }
        }

        public void ShowRosterForTeam(string abbr)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            if (rosters == null || !rosters.TryGetValue(abbr, out var list) || list == null || list.Count == 0)
            {
                if (messageText != null)
                {
                    messageText.text = $"No roster data for {abbr}";
                    messageText.gameObject.SetActive(true);
                }
                Debug.LogWarning($"No roster data for {abbr}");
                return;
            }

            messageText?.gameObject.SetActive(false);

            foreach (PlayerData p in list.OrderBy(pl => pl.pos).ThenByDescending(pl => pl.ovr))
            {
                var row = Instantiate(playerRowPrefab, content);
                row.Set(p);
            }

            Debug.Log($"{abbr} roster count: {list.Count}");
        }
    }
}
