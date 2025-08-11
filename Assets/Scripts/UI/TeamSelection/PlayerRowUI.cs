using TMPro;
using UnityEngine;
using GridironGM.Data;

namespace GridironGM.UI
{
    public class PlayerRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text posText;
        [SerializeField] private TMP_Text ovrText;

        public void Set(PlayerData data)
        {
            if (data == null) return;
            if (nameText != null) nameText.text = $"{data.first} {data.last}";
            if (posText != null) posText.text = data.pos;
            if (ovrText != null) ovrText.text = data.ovr.ToString();
        }
    }
}
