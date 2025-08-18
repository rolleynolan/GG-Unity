using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // ← THIS is what allows TMP_InputField

public class GMCreatorUI : MonoBehaviour
{
    public TMP_InputField gmNameInput; // ← FIX: use TMP_InputField

    public void OnConfirmGMPressed()
    {
        string gmName = gmNameInput.text;

        if (string.IsNullOrWhiteSpace(gmName))
        {
            Debug.LogWarning("GM name is empty.");
            return;
        }

        PlayerPrefs.SetString("CurrentGMName", gmName); // Temp storage
        SceneManager.LoadScene("TeamSelection");
    }


    public void OnBackPressed()
    {
        SceneManager.LoadScene("NewGameSetup");
    }
}
