using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool isGamePaused;
    public GameObject pauseMenu;

    private void Start()
    {
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            print("paused");
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;

            isGamePaused = true;
        }
    }

    public void Resume()
    {
        print("resumed");
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;

        isGamePaused = false;
    }

    public void MainMenu()
    {
        print("back to menu");
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void Quit()
    {
        print("would quit game in build");
        Application.Quit();
    }

    public void Restart()
    {
        print("restart Game scene");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }

}
