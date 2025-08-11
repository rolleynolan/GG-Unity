using TMPro;
using UnityEngine;

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
            if (!nameText || !posText || !ovrText) TryAutoWire();

            if (nameText) nameText.text = $"{p.first} {p.last}";
            if (posText)  posText.text  = p.pos;
            if (ovrText)  ovrText.text  = p.ovr.ToString();
            if (ageText)  ageText.text  = p.age > 0 ? p.age.ToString() : "";
        }

        private void TryAutoWire()
        {
            if (!nameText) nameText = transform.Find("NameText")?.GetComponent<TMP_Text>();
            if (!posText)  posText  = transform.Find("PosText")?.GetComponent<TMP_Text>();
            if (!ovrText)  ovrText  = transform.Find("OvrText")?.GetComponent<TMP_Text>();
            if (!ageText)
            {
                ageText = transform.Find("AgeText")?.GetComponent<TMP_Text>();
                if (!ageText) ageText = transform.Find("NumText")?.GetComponent<TMP_Text>(); // support your earlier naming
            }
        }

        private void Awake() { TryAutoWire(); }
#if UNITY_EDITOR
        private void OnValidate() { TryAutoWire(); }
#endif
    }
}
