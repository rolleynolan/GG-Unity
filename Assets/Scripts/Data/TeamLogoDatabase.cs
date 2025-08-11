using System.Collections.Generic;
using UnityEngine;

namespace GridironGM.Data
{
    [System.Serializable]
    public class TeamLogoEntry
    {
        public string abbreviation;
        public Sprite sprite;
    }

    public class TeamLogoDatabase : ScriptableObject
    {
        [SerializeField] private List<TeamLogoEntry> entries = new List<TeamLogoEntry>();
        private Dictionary<string, Sprite> map;

        private static TeamLogoDatabase _instance;
        public static TeamLogoDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<TeamLogoDatabase>("TeamLogoDB"); // Assets/Resources/TeamLogoDB.asset
                return _instance;
            }
        }

        private void OnEnable()
        {
            if (map == null)
            {
                map = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
                foreach (var e in entries)
                    if (!string.IsNullOrWhiteSpace(e.abbreviation) && e.sprite != null)
                        map[e.abbreviation] = e.sprite;
            }
        }

        public Sprite Get(string abbr)
        {
            if (string.IsNullOrEmpty(abbr)) return null;
            if (map == null) OnEnable();
            return map != null && map.TryGetValue(abbr, out var s) ? s : null;
        }

    #if UNITY_EDITOR
        // Allow the editor script to set entries
        public void SetEntries(List<TeamLogoEntry> newEntries)
        {
            entries = newEntries;
            map = null;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    #endif
    }
}

