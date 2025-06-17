using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnNewGamePressed()
    {
        SceneManager.LoadScene("NewGameSetup");
    }

    public void OnLoadGamePressed()
    {
        SceneManager.LoadScene("TeamSelection");
    }

    public void OnSettingsPressed()
    {
        Debug.Log("Settings opened");
    }

    public void OnExitGamePressed()
    {
        Application.Quit();
    }
}
