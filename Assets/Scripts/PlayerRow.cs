using UnityEngine;
using TMPro;

public class PlayerRow : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text positionText;
    public TMP_Text collegeText;
    public TMP_Text overallText;

    public void SetData(PlayerData data)
    {
        if (data == null) return;

        if (nameText != null) nameText.text = data.name;
        if (positionText != null) positionText.text = data.position;
        if (collegeText != null) collegeText.text = data.college;
        if (overallText != null) overallText.text = data.overall.ToString();
    }
}
