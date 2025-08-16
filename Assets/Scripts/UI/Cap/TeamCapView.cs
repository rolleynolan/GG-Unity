using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GG.Infra;
using GG.Bridge.Validation;

namespace GG.UI.Cap
{
    [Serializable]
    public class CapRow
    {
        public string PlayerName;
        public string TeamAbbr;
        public int Year;
        public long Base;
        public long SigningProrated;
        public long RosterBonus;
        public long WorkoutBonus;
        public long CapHit;
        public long DeadCap;
    }

    [Serializable]
    public class CapSheet
    {
        public List<CapRow> Rows = new();
        public long TeamTotalCapHit;
        public long TeamTotalDeadCap;
    }

    public class TeamCapView : MonoBehaviour
    {
        [SerializeField] private TMP_Text content;
        public string TeamAbbr;
        public int Year;

        void OnEnable()
        {
            if (!content)
            {
                var go = new GameObject("CapText", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                content = go.AddComponent<TMP_Text>();
            }
            Render();
        }

        void Render()
        {
            try
            {
                // Support either "cap/capsheet_YYYY.json" or "/data/cap/capsheet_YYYY.json"
                var relative = $"cap/capsheet_{Year}.json";
                var sheet = DataIO.LoadJson<CapSheet>(relative);

                var list = new List<string>();
                foreach (var r in sheet.Rows)
                {
                    if (!string.IsNullOrEmpty(TeamAbbr) && !string.Equals(r.TeamAbbr, TeamAbbr, StringComparison.OrdinalIgnoreCase))
                        continue;
                    list.Add($"{r.PlayerName,-20}  {Fmt(r.CapHit)}  (dead {Fmt(r.DeadCap)})");
                }

                if (list.Count == 0) content.text = "No cap rows found.";
                else content.text = string.Join("\n", list);
            }
            catch (Exception ex)
            {
                GGLog.Error("Failed to load capsheet", ex);
                content.text = "Cap sheet not available.";
            }
        }

        static string Fmt(long v) => "$" + (v / 1_000_000f).ToString("0.0") + "M";
    }
}
