using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class UpgradeData
{
    public int healthUpgrades = 0;
    public int damageUpgrades = 0;
    public int speedUpgrades = 0;
    public int jumpUpgrades = 0;
    public int armorLevel = 0;
    public WeaponData equippedWeapon;
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<GameManager>();
            return instance;
        }
    }

    // Set by SceneLoadTrigger before loading a scene.
    // SpawnPoint objects read this to position the player on arrival.
    public static string nextSpawnPointId;

    [Header("References")]
    public AudioSource audioSource;
    public HUD hud;
    public DialogueBoxController dialogueBoxController;
    public AudioTrigger gameMusic;
    public AudioTrigger gameAmbience;

    [System.NonSerialized] public int enemiesKilled = 0;
    [System.NonSerialized] public int totalCoinsCollected = 0;

    // Lifetime stats (persisted via PlayerPrefs)
    private const string PREF_TOTAL_DAMAGE = "LifetimeTotalDamage";
    private const string PREF_TOTAL_COINS = "LifetimeTotalCoins";
    private const string PREF_TOTAL_DEATHS = "LifetimeTotalDeaths";
    private const string PREF_HIGHEST_ROUND = "LifetimeHighestRound";

    public int LifetimeTotalDamage => PlayerPrefs.GetInt(PREF_TOTAL_DAMAGE, 0);
    public int LifetimeTotalCoins => PlayerPrefs.GetInt(PREF_TOTAL_COINS, 0);
    public int LifetimeTotalDeaths => PlayerPrefs.GetInt(PREF_TOTAL_DEATHS, 0);
    public int LifetimeHighestRound => PlayerPrefs.GetInt(PREF_HIGHEST_ROUND, 0);

    public void AddLifetimeDamage(int amount)
    {
        PlayerPrefs.SetInt(PREF_TOTAL_DAMAGE, LifetimeTotalDamage + amount);
    }

    public void AddLifetimeCoins(int amount)
    {
        PlayerPrefs.SetInt(PREF_TOTAL_COINS, LifetimeTotalCoins + amount);
    }

    public void RecordDeath()
    {
        PlayerPrefs.SetInt(PREF_TOTAL_DEATHS, LifetimeTotalDeaths + 1);
        // Also check highest round
        int currentRound = RoundManager.Instance != null ? RoundManager.Instance.CurrentRound : 0;
        if (currentRound > LifetimeHighestRound)
            PlayerPrefs.SetInt(PREF_HIGHEST_ROUND, currentRound);
        PlayerPrefs.Save();
    }

    [Header("Persistent Player State")]
    public int coins = 0;
    public UpgradeData upgradeData = new UpgradeData();
    public WeaponData equippedWeapon;

    public Dictionary<string, Sprite> inventory = new Dictionary<string, Sprite>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "PauseMenu" && obj.scene.isLoaded)
            {
                DontDestroyOnLoad(obj);
                break;
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hud = FindFirstObjectByType<HUD>();
        dialogueBoxController = FindFirstObjectByType<DialogueBoxController>();

        foreach (DialogueTrigger trigger in FindObjectsByType<DialogueTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            trigger.EnableInteractAction();

        audioSource = GetComponent<AudioSource>();

        GameObject musicObj = GameObject.FindGameObjectWithTag("GameMusic");
        if (musicObj != null) gameMusic = musicObj.GetComponent<AudioTrigger>();

        GameObject ambienceObj = GameObject.FindGameObjectWithTag("GameAmbience");
        if (ambienceObj != null) gameAmbience = ambienceObj.GetComponent<AudioTrigger>();

        StartCoroutine(AssignConfinerNextFrame());
    }

    private System.Collections.IEnumerator AssignConfinerNextFrame()
    {
        yield return null; // wait one frame for scene to fully initialize

        GameObject confinerObj = GameObject.FindGameObjectWithTag("Confiner");
        CinemachineCamera cinemachineCam = FindFirstObjectByType<CinemachineCamera>();
        if (confinerObj != null && cinemachineCam != null)
        {
            var confiner = cinemachineCam.GetComponent<CinemachineConfiner2D>();
            if (confiner != null)
            {
                confiner.BoundingShape2D = confinerObj.GetComponent<Collider2D>();
                confiner.InvalidateBoundingShapeCache();
            }
        }
    }

    // Coins
    public void AddCoins(int amount)
    {
        coins += amount;
        totalCoinsCollected += amount;
        AddLifetimeCoins(amount);
        if (hud != null) hud.UpdateCoinDisplay();
    }

    public void RegisterEnemyKill()
    {
        enemiesKilled++;
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        return true;
    }

    // Inventory
    public void GetInventoryItem(string itemName, Sprite image)
    {
        if (inventory.ContainsKey(itemName)) return;
        inventory.Add(itemName, image);
        if (image != null && hud != null)
            hud.SetInventoryImage(inventory[itemName]);
    }

    public void ResetRun()
    {
        coins = 0;
        enemiesKilled = 0;
        totalCoinsCollected = 0;
        inventory.Clear();
        upgradeData = new UpgradeData();
        if (hud != null)
        {
            hud.UpdateCoinDisplay();
            hud.RefreshHearts();
        }
    }

    public void RemoveInventoryItem(string itemName)
    {
        inventory.Remove(itemName);
        if (hud != null) hud.SetInventoryImage(hud.blankUI);
    }

    public void ClearInventory()
    {
        inventory.Clear();
        if (hud != null) hud.SetInventoryImage(hud.blankUI);
    }

    // Upgrades
    public void UpgradeHealth()
    {
        PlayerStats stats = NewPlayer.Instance.GetComponent<PlayerStats>();
        if (stats.MaxHealth >= PlayerStats.MaxPossibleHealth) return;
        upgradeData.healthUpgrades++;
        HealthComponent health = NewPlayer.Instance.GetComponent<HealthComponent>();
        health.Initialize(stats.MaxHealth);
        if (hud != null) hud.RefreshHearts();
    }

    public void UpgradeDamage() => upgradeData.damageUpgrades++;
    public void UpgradeSpeed() => upgradeData.speedUpgrades++;
    public void UpgradeJump() => upgradeData.jumpUpgrades++;
    public void UpgradeArmor() => upgradeData.armorLevel++;

    public void EquipWeapon(WeaponData weapon)
    {
        equippedWeapon = weapon;
        NewPlayer.Instance.GetComponent<WeaponComponent>().EquipWeapon(weapon);
    }
}