using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Persists between scenes. Tracks the round number, scales enemy difficulty,
// counts living enemies, and spawns the exit key when the arena is cleared.
// Call StartNewRound() when the player enters the arena.
public class RoundManager : MonoBehaviour
{
    private static RoundManager instance;
    public static RoundManager Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<RoundManager>();
            return instance;
        }
    }

    [Header("Round Settings")]
    [SerializeField] private int bossRoundInterval = 3;
    public int CurrentRound { get; private set; } = 0;
    public bool IsBossRound => CurrentRound > 0 && CurrentRound % bossRoundInterval == 0;
    public int EnemiesRemaining => livingEnemyCount;

    [Header("Difficulty Scaling")]
    [SerializeField] private float healthScalePerRound = 0.15f;  // +15% enemy HP per round
    [SerializeField] private float speedScalePerRound = 0.05f; // +5%  enemy speed per round
    [SerializeField] private float damageScalePerRound = 0.05f;  // +5% enemy damage per round

    public float HealthMultiplier => CurrentRound <= 0 ? 1f : 1f + (CurrentRound - 1) * healthScalePerRound;
    public float SpeedMultiplier => 1f + (CurrentRound - 1) * speedScalePerRound;
    public float DamageMultiplier => 1f + (CurrentRound - 1) * damageScalePerRound;

    [Header("Key Spawning")]
    // Assign the key prefab here. Tag an empty GameObject "KeySpawnPoint" in the arena scene.
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private float keySpawnDelay = 1f;

    [Header("Events")]
    public UnityEvent onRoundStarted;
    public UnityEvent onAllEnemiesDead;

    private int livingEnemyCount = 0;
    private Vector3 lastEnemyPosition;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Call this when the player enters the arena door
    public void StartNewRound()
    {
        CurrentRound++;
        livingEnemyCount = 0;
        onRoundStarted?.Invoke();
        Debug.Log($"[RoundManager] Round {CurrentRound} started. Boss round: {IsBossRound}");

        // Re-initialize player health to new max (accounts for round-based bonus)
        if (NewPlayer.Instance != null)
        {
            HealthComponent health = NewPlayer.Instance.GetComponent<HealthComponent>();
            PlayerStats stats = NewPlayer.Instance.GetComponent<PlayerStats>();
            if (health != null && stats != null)
                health.Initialize(stats.MaxHealth);
        }

        if (GameManager.Instance.hud != null)
        {
            GameManager.Instance.hud.UpdateRoundText();
            GameManager.Instance.hud.RefreshHearts();
        }
    }

    public void RegisterEnemy()
    {
        livingEnemyCount++;
    }

    public void UnregisterEnemy(Vector3 position)
    {
        livingEnemyCount = Mathf.Max(0, livingEnemyCount - 1);
        lastEnemyPosition = position;

        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        bool queueEmpty = spawner == null || spawner.IsQueueEmpty;

        if (livingEnemyCount <= 0 && queueEmpty)
        {
            onAllEnemiesDead?.Invoke();
            StartCoroutine(SpawnKeyAfterDelay());
        }
    }

    private IEnumerator SpawnKeyAfterDelay()
    {
        yield return new WaitForSeconds(keySpawnDelay);
        if (keyPrefab == null) yield break;
        Instantiate(keyPrefab, lastEnemyPosition, Quaternion.identity);
    }

    public void ResetRounds()
    {
        CurrentRound = 0;
        livingEnemyCount = 0;
    }


}