using System.Linq;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GridironGM.UI.TeamSelection
{
    public class PlayerRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text posText;
        [SerializeField] private TMP_Text ovrText;
        [SerializeField] private TMP_Text ageText; // optional

        public void Set(GridironGM.Data.PlayerData p)
        {
            if (p == null) { gameObject.SetActive(false); return; }

            // Try to auto-wire if fields are not set
            if (!nameText || !posText || !ovrText)
                TryAutoWireOrCreate();

            if (!nameText || !posText || !ovrText)
            {
                Debug.LogError("[PlayerRowUI] Missing TMP fields even after auto-wire. Check prefab children or assign in Inspector.");
                return;
            }

            nameText.text = $"{p.first} {p.last}";
            posText.text  = p.pos;
            ovrText.text  = p.ovr.ToString();
            if (ageText) ageText.text = p.age > 0 ? p.age.ToString() : "";
        }

        private void TryAutoWireOrCreate()
        {
            // 1) Try to find by expected names
            if (!nameText) nameText = transform.Find("NameText")?.GetComponent<TMP_Text>();
            if (!posText)  posText  = transform.Find("PosText")?.GetComponent<TMP_Text>();
            if (!ovrText)  ovrText  = transform.Find("OvrText")?.GetComponent<TMP_Text>();
            if (!ageText)
            {
                ageText = transform.Find("AgeText")?.GetComponent<TMP_Text>();
                if (!ageText) ageText = transform.Find("NumText")?.GetComponent<TMP_Text>(); // support older naming
            }

            // 2) If still missing, grab any child TMPs in order: Name, Pos, Ovr, Age
            if (!nameText || !posText || !ovrText)
            {
                var tmps = GetComponentsInChildren<TMP_Text>(true).ToList();
                if (tmps.Count >= 3)
                {
                    if (!nameText) nameText = tmps[0];
                    if (!posText)  posText  = tmps[1];
                    if (!ovrText)  ovrText  = tmps[2];
                    if (!ageText && tmps.Count >= 4) ageText = tmps[3];
                }
            }
        }

        private void Awake()     => TryAutoWireOrCreate();
#if UNITY_EDITOR
        private void OnValidate() => TryAutoWireOrCreate();
#endif
    }
}

