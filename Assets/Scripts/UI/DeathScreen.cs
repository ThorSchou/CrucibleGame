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
        // Reset everything and start fresh at round 1
        RoundManager.Instance.ResetRounds();
        GameManager.Instance.ResetRun();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Arena_Dragons");
    }

    public void MainMenu()
    {
        RoundManager.Instance.ResetRounds();
        GameManager.Instance.ResetRun();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}