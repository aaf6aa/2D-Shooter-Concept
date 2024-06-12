using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void ChangeLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainScene" && Input.GetKey(KeyCode.Escape))
        {
            Exit();
        }
        else if ((SceneManager.GetActiveScene().name == "GameScene" || SceneManager.GetActiveScene().name == "HelpMenu") && Input.GetKey(KeyCode.Escape))
        {
            ChangeLevel("MainMenu");
        }
    }
}
