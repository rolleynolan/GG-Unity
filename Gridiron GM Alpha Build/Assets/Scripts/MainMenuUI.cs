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
        Debug.Log("Load Game pressed");
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
