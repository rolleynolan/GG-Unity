using UnityEngine;
using UnityEngine.EventSystems;

public class TeamRowUI : MonoBehaviour, IPointerClickHandler
{
    public string teamAbbreviation;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Selected team: " + teamAbbreviation);
        PlayerPrefs.SetString("selected_team", teamAbbreviation);
        // You can call a method on TeamSelectionUI to update roster view, etc.
    }
}
