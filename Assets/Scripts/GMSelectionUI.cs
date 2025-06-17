using UnityEngine;
using UnityEngine.SceneManagement;

public class GMSelectionUI : MonoBehaviour
{
    public void OnCreateNewGMPressed()
    {
        SceneManager.LoadScene("TeamSelection");
    }

    public void OnUseExistingGMPressed()
    {
        Debug.Log("Use Existing GM pressed");
    }

    public void OnBackPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
