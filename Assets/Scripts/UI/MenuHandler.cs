using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string arenaSceneName = "Arena";
    [SerializeField] private string menuSceneName = "MainMenu";

    // -------------------------------------------------------------------------
    // Main menu buttons
    // -------------------------------------------------------------------------

    // Call this from your "New Game" button
    public void StartNewGame()
    {
        // Reset persistent state so a new run starts clean
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = 0;
            GameManager.Instance.upgradeData = new UpgradeData();
            GameManager.Instance.ClearInventory();
        }

        // Reset round counter
        if (RoundManager.Instance != null)
            RoundManager.Instance.ResetRounds();

        SceneManager.LoadScene(arenaSceneName);
    }

    // Call this from your "Quit" button
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // -------------------------------------------------------------------------
    // Pause menu buttons
    // -------------------------------------------------------------------------

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}