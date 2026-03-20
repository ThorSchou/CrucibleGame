using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject statsOverlay;

    public void PlayGame()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.ResetRounds();
        if (GameManager.Instance != null)
            GameManager.Instance.ResetRun();
        SceneManager.LoadScene("Hub");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void OpenStats()
    {
        if (statsOverlay != null)
            statsOverlay.SetActive(true);
    }

    public void OpenInfo()
    {
        SceneManager.LoadScene("Info");
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
