using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class MainMenuController : MonoBehaviour
{
    public void OnNewGamePressed()
    {
        // Change to "TeamSelection" if that's your flow
        SceneManager.LoadScene("NewGameSetup");
    }

    public void OnLoadGamePressed() => Debug.Log("Load Game pressed (stub)");
    public void OnSettingsPressed() => Debug.Log("Settings opened (stub)");

    public void OnExitGamePressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
