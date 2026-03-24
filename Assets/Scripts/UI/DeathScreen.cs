using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Button defaultButton;

    public void Show()
    {
        gameObject.SetActive(true);

        GameManager.Instance.RecordDeath();

        roundText.text = $"ROUND {RoundManager.Instance.CurrentRound}";
        enemiesText.text = $"ENEMIES KILLED: {GameManager.Instance.enemiesKilled}";
        coinsText.text = $"COINS COLLECTED: {GameManager.Instance.totalCoinsCollected}";

        // Auto-select the default button so keyboard/Enter works
        if (defaultButton != null)
            EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
    }

    public void Restart()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;

        // Destroy all persistent objects so they recreate fresh
        Destroy(NewPlayer.Instance.gameObject);
        Destroy(GameManager.Instance.gameObject);
        Destroy(RoundManager.Instance.gameObject);

        // Find and destroy persistent cameras and HUD
        GameObject cameras = GameObject.Find("Cameras");
        GameObject hud = GameObject.Find("HUD");
        if (cameras != null) Destroy(cameras);
        if (hud != null) Destroy(hud);

        SceneManager.LoadScene("Hub");
    }

    public void MainMenu()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;

        // Destroy all persistent objects so they recreate fresh
        Destroy(NewPlayer.Instance.gameObject);
        Destroy(GameManager.Instance.gameObject);
        Destroy(RoundManager.Instance.gameObject);

        GameObject cameras = GameObject.Find("Cameras");
        GameObject hud = GameObject.Find("HUD");
        GameObject pauseMenu = GameObject.Find("PauseMenu");
        if (cameras != null) Destroy(cameras);
        if (hud != null) Destroy(hud);
        if (pauseMenu != null) Destroy(pauseMenu);

        SceneManager.LoadScene("MainMenu");
    }
}