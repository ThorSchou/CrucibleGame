using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    [SerializeField] private Image[] hearts;        // assign all 10 heart slots in order
    [SerializeField] private Image inventorySlot;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Flame Sprites (Health)")]
    [SerializeField] private Sprite[] flameFrames;      // trifire-flame-spritesheet frames
    [SerializeField] private Sprite[] trifireFrames;    // trifire-all-spritesheet frames (armored)
    [SerializeField] private Sprite heartEmpty;          // empty health slot sprite
    [SerializeField] private float heartAnimFPS = 12f;

    [Header("Coin Icon")]
    [SerializeField] private Sprite[] coinFrames;       // coin-spritesheet frames
    [SerializeField] private Image coinIcon;             // animated coin Image next to text
    [SerializeField] private float coinAnimFPS = 10f;

    [Header("Key Icon")]
    [SerializeField] private Sprite[] keyFrames;        // boss-key-spritesheet frames
    [SerializeField] private float keyAnimFPS = 10f;

    [System.NonSerialized] public Sprite blankUI;
    [System.NonSerialized] public string loadSceneName;

    private HealthComponent playerHealth;
    private PlayerStats playerStats;
    private float coinsEased;
    private float coinsThreshold;

    // Animation state
    private int heartFrame;
    private int coinFrame;
    private int keyFrame;
    private float heartTimer;
    private float coinTimer;
    private float keyTimer;

    void Start()
    {
        playerHealth = NewPlayer.Instance.GetComponent<HealthComponent>();
        playerStats = NewPlayer.Instance.GetComponent<PlayerStats>();
        blankUI = inventorySlot.sprite;
        coinsEased = GameManager.Instance.coins;
        coinsThreshold = coinsEased + 1;

        GameManager.Instance.hud = this;
        UpdateRoundText();
        inventorySlot.gameObject.SetActive(false);

        // Position round text at top-center of screen
        if (roundText != null)
        {
            RectTransform rt = roundText.rectTransform;
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -15f);
            rt.sizeDelta = new Vector2(300f, 40f);
        }

        // Hide coin icon if no frames assigned (prevents white box)
        if (coinIcon != null)
        {
            if (coinFrames != null && coinFrames.Length > 0)
                coinIcon.sprite = coinFrames[0];
            else
                coinIcon.gameObject.SetActive(false);
        }

        // Position coin icon right next to coin text, horizontally aligned top-right
        if (coinIcon != null && coinText != null)
        {
            RectTransform iconRT = coinIcon.rectTransform;
            RectTransform textRT = coinText.rectTransform;

            // Make both direct children of the same parent (TopRight)
            Transform parent = textRT.parent;
            iconRT.SetParent(parent, false);

            // Anchor both to top-right
            textRT.anchorMin = new Vector2(1f, 1f);
            textRT.anchorMax = new Vector2(1f, 1f);
            textRT.pivot = new Vector2(1f, 1f);
            textRT.anchoredPosition = new Vector2(-5f, -5f);
            textRT.sizeDelta = new Vector2(80f, 30f);

            iconRT.anchorMin = new Vector2(1f, 1f);
            iconRT.anchorMax = new Vector2(1f, 1f);
            iconRT.pivot = new Vector2(1f, 1f);
            iconRT.anchoredPosition = new Vector2(-88f, -2f); // just left of text
            iconRT.sizeDelta = new Vector2(28f, 28f);
        }

        // Position inventory slot below round text at top-center
        if (inventorySlot != null)
        {
            RectTransform rt = inventorySlot.rectTransform;
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -55f);
            rt.sizeDelta = new Vector2(40f, 40f);
        }

        // Resize heart slots for trifire sprites (wider than plain flames)
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            RectTransform hrt = hearts[i].rectTransform;
            hrt.sizeDelta = new Vector2(45f, 40f);
        }

        // Increase HealthBar spacing for wider trifire slots
        if (hearts.Length > 0 && hearts[0] != null)
        {
            HorizontalLayoutGroup hlg = hearts[0].transform.parent.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) hlg.spacing = 6f;
        }

        StartCoroutine(LateStart());
    }

    private System.Collections.IEnumerator LateStart()
    {
        yield return null;
        RefreshHearts();
    }

    void Update()
    {
        UpdateCoinDisplay();
        AnimateHearts();
        AnimateCoinIcon();
        AnimateKey();
    }

    // -------------------------------------------------------------------------
    // Health
    // -------------------------------------------------------------------------

    public void RefreshHearts()
    {
        if (playerHealth == null) playerHealth = NewPlayer.Instance?.GetComponent<HealthComponent>();
        if (playerStats == null) playerStats = NewPlayer.Instance?.GetComponent<PlayerStats>();
        if (playerHealth == null || playerStats == null) return;
        // Each slot represents 2 HP:
        //   2 HP = trifire (flame + triangle)
        //   1 HP = flame only (triangle removed)
        //   0 HP = faded flame
        int hp = playerHealth.CurrentHealth;
        int maxHp = playerStats.MaxHealth;
        int slotsNeeded = Mathf.CeilToInt(maxHp / 2f);

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            bool withinMax = i < slotsNeeded;
            hearts[i].gameObject.SetActive(withinMax);

            if (withinMax)
            {
                int hpInSlot = Mathf.Clamp(hp - i * 2, 0, 2);

                if (hpInSlot == 2)
                {
                    // Full: trifire (flame + triangle)
                    hearts[i].color = Color.white;
                    if (trifireFrames != null && trifireFrames.Length > 0)
                        hearts[i].sprite = trifireFrames[heartFrame % trifireFrames.Length];
                }
                else if (hpInSlot == 1)
                {
                    // Half: flame only (triangle broken off)
                    hearts[i].color = Color.white;
                    if (flameFrames != null && flameFrames.Length > 0)
                        hearts[i].sprite = flameFrames[heartFrame % flameFrames.Length];
                }
                else
                {
                    // Empty: faded flame
                    hearts[i].color = new Color(1f, 1f, 1f, 0.25f);
                    if (flameFrames != null && flameFrames.Length > 0)
                        hearts[i].sprite = flameFrames[heartFrame % flameFrames.Length];
                }
            }
        }
    }

    private void AnimateHearts()
    {
        if (playerHealth == null) return;
        int maxFrames = Mathf.Max(
            trifireFrames != null ? trifireFrames.Length : 0,
            flameFrames != null ? flameFrames.Length : 0);
        if (maxFrames == 0) return;

        heartTimer += Time.deltaTime;
        if (heartTimer < 1f / heartAnimFPS) return;
        heartTimer -= 1f / heartAnimFPS;
        heartFrame = (heartFrame + 1) % maxFrames;

        int hp = playerHealth.CurrentHealth;
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null || !hearts[i].gameObject.activeSelf) continue;
            int hpInSlot = Mathf.Clamp(hp - i * 2, 0, 2);

            if (hpInSlot == 2 && trifireFrames != null && trifireFrames.Length > 0)
                hearts[i].sprite = trifireFrames[heartFrame % trifireFrames.Length];
            else if (hpInSlot >= 0 && flameFrames != null && flameFrames.Length > 0)
                hearts[i].sprite = flameFrames[heartFrame % flameFrames.Length];
        }
    }

    public void HealthBarHurt()
    {
        animator.SetTrigger("hurt");
        RefreshHearts();
    }

    // -------------------------------------------------------------------------
    // Coins
    // -------------------------------------------------------------------------

    private void AnimateCoinIcon()
    {
        if (coinIcon == null || coinFrames == null || coinFrames.Length == 0) return;

        coinTimer += Time.deltaTime;
        if (coinTimer < 1f / coinAnimFPS) return;
        coinTimer -= 1f / coinAnimFPS;
        coinFrame = (coinFrame + 1) % coinFrames.Length;
        coinIcon.sprite = coinFrames[coinFrame];
    }

    public void UpdateCoinDisplay()
    {
        coinsEased += (GameManager.Instance.coins - coinsEased) * Time.deltaTime * 5f;
        coinText.text = Mathf.Round(coinsEased).ToString();

        if (coinsEased >= coinsThreshold)
        {
            animator.SetTrigger("getGem");
            coinsThreshold = coinsEased + 1;
        }
    }

    // -------------------------------------------------------------------------
    // Round
    // -------------------------------------------------------------------------

    public void UpdateRoundText()
    {
        if (roundText != null)
        {
            roundText.text = "Round " + RoundManager.Instance.CurrentRound;
            roundText.alignment = TextAlignmentOptions.Center;
        }
    }

    // -------------------------------------------------------------------------
    // Inventory
    // -------------------------------------------------------------------------

    public void SetInventoryImage(Sprite image)
    {
        if (image == null || image == blankUI)
        {
            inventorySlot.gameObject.SetActive(false);
        }
        else
        {
            inventorySlot.gameObject.SetActive(true);
            // Use animated key frames if available
            if (keyFrames != null && keyFrames.Length > 0)
            {
                keyFrame = 0;
                keyTimer = 0f;
                inventorySlot.sprite = keyFrames[0];
            }
            else
            {
                inventorySlot.sprite = image;
            }
        }
    }

    private void AnimateKey()
    {
        if (inventorySlot == null || !inventorySlot.gameObject.activeSelf) return;
        if (keyFrames == null || keyFrames.Length == 0) return;

        keyTimer += Time.deltaTime;
        if (keyTimer < 1f / keyAnimFPS) return;
        keyTimer -= 1f / keyAnimFPS;
        keyFrame = (keyFrame + 1) % keyFrames.Length;
        inventorySlot.sprite = keyFrames[keyFrame];
    }

    // -------------------------------------------------------------------------
    // Scene transition � called by animation event on screen-cover animation
    // -------------------------------------------------------------------------

    void ResetScene()
    {
        if (GameManager.Instance.inventory.ContainsKey("reachedCheckpoint"))
            NewPlayer.Instance.ResetLevel();
        else
            SceneManager.LoadScene(loadSceneName);
    }
}