using TMPro;
using UnityEngine;

namespace GridironGM.UI
{
    public class PlayerRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text posText;
        [SerializeField] private TMP_Text ovrText;

        public void Set(GridironGM.Data.PlayerData p)
        {
            if (p == null) return;
            if (nameText != null) nameText.text = $"{p.first} {p.last}";
            if (posText != null) posText.text = p.pos;
            if (ovrText != null) ovrText.text = p.ovr.ToString();
        }
    }
}
