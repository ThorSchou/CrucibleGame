using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class StatsOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalDamageText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private TextMeshProUGUI totalDeathsText;
    [SerializeField] private TextMeshProUGUI highestRoundText;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject menuButtonsParent;
    void OnEnable()
    {
        // Read directly from PlayerPrefs so it works even without GameManager
        totalDamageText.text = PlayerPrefs.GetInt("LifetimeTotalDamage", 0).ToString();
        totalCoinsText.text = PlayerPrefs.GetInt("LifetimeTotalCoins", 0).ToString();
        totalDeathsText.text = PlayerPrefs.GetInt("LifetimeTotalDeaths", 0).ToString();
        highestRoundText.text = PlayerPrefs.GetInt("LifetimeHighestRound", 0).ToString();

        if (backButton != null)
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);

        if (menuButtonsParent != null)
            menuButtonsParent.SetActive(false);
    }

    public void Close()
    {
        if (menuButtonsParent != null)
        {
            menuButtonsParent.SetActive(true);
            var firstButton = menuButtonsParent.GetComponentInChildren<Button>();
            if (firstButton != null)
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
        gameObject.SetActive(false);
    }
}
