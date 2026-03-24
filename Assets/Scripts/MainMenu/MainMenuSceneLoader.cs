using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuSceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject statsOverlay;
    [SerializeField] private GameObject defaultSelected;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (defaultSelected != null)
            EventSystem.current.SetSelectedGameObject(defaultSelected);
    }

    public void PlayGame()
    {
        bool hasRunInProgress = RoundManager.Instance != null && RoundManager.Instance.CurrentRound > 0;

        if (!hasRunInProgress)
        {
            if (RoundManager.Instance != null)
                RoundManager.Instance.ResetRounds();
            if (GameManager.Instance != null)
                GameManager.Instance.ResetRun();
        }

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
