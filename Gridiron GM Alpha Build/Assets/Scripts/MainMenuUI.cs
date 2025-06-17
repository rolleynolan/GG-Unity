using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnNewGameClicked()
    {
        // Load GM selection scene or show a panel
    }

    public void OnLoadGameClicked()
    {
        // Load save selection logic (to be implemented)
    }

    public void OnSettingsClicked()
    {
        // Show settings panel (future)
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }
}
