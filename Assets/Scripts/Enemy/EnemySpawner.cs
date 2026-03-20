using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawn budget system:
// - Each round has a total enemy quota
// - Enemies spawn gradually to maintain a target number alive at once
// - Spawn rate accelerates the longer the round goes
// - Max alive cap increases each round
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyWave
    {
        public GameObject enemyPrefab;
        [Tooltip("How many of this enemy to spawn at base round (round 1)")]
        public int baseCount = 2;
        [Tooltip("How many extra of this enemy to add per round")]
        public int extraPerRound = 1;
        [Tooltip("From which round should this enemy start appearing?")]
        public int firstAppearsOnRound = 1;
    }

    [Header("Settings")]
    [SerializeField] private bool autoSpawnOnRoundStart = true;

    [Header("Enemies")]
    [SerializeField] private List<EnemyWave> waves;

    [Header("Spawn Budget")]
    [SerializeField] private int startingMaxAlive = 2;   // max enemies alive at round start
    [SerializeField] private int maxAliveIncreasePerRound = 1; // max alive increases each round
    [SerializeField] private int hardMaxAlive = 8;   // never exceed this many at once
    [SerializeField] private float startSpawnInterval = 3f;  // seconds between spawns at round start
    [SerializeField] private float minSpawnInterval = 0.5f;// fastest possible spawn rate
    [SerializeField] private float spawnAcceleration = 0.95f; // interval multiplies by this each spawn (lower = faster ramp)

    [Header("Camera Spawn Settings")]
    [SerializeField] private float groundY = -3f;
    [SerializeField] private float spawnHeightAboveGround = 2f;
    [SerializeField] private float cameraEdgeOffset = 2f;
    [SerializeField] private Transform spawnLeft;   // left edge of arena
    [SerializeField] private Transform spawnRight;  // right edge of arena

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    public bool IsQueueEmpty => spawnedCount >= totalQuota;

    private List<GameObject> spawnQueue = new List<GameObject>();
    private int aliveCount = 0;
    private int spawnedCount = 0;
    private int totalQuota = 0;
    private float currentInterval;
    private bool roundActive = false;
    private int maxAlive;

    void Start()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (tagged.Length > 0)
        {
            spawnPoints = new Transform[tagged.Length];
            for (int i = 0; i < tagged.Length; i++)
                spawnPoints[i] = tagged[i].transform;
        }

        if (autoSpawnOnRoundStart)
            RoundManager.Instance.onRoundStarted.AddListener(StartRound);

        if (RoundManager.Instance.CurrentRound == 0)
            RoundManager.Instance.StartNewRound();
    }

    void OnDestroy()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.onRoundStarted.RemoveListener(StartRound);
    }

    // -------------------------------------------------------------------------
    // Round start — build the queue and begin spawning
    // -------------------------------------------------------------------------

    public void StartRound()
    {
        int currentRound = RoundManager.Instance.CurrentRound;

        // Build flat spawn queue for this round
        spawnQueue.Clear();
        foreach (EnemyWave wave in waves)
        {
            if (currentRound < wave.firstAppearsOnRound) continue;
            int count = wave.baseCount + (currentRound - wave.firstAppearsOnRound) * wave.extraPerRound;
            count = Mathf.Min(count, 20);
            for (int i = 0; i < count; i++)
                spawnQueue.Add(wave.enemyPrefab);
        }

        Shuffle(spawnQueue);

        totalQuota = spawnQueue.Count;
        spawnedCount = 0;
        aliveCount = 0;
        currentInterval = startSpawnInterval;
        maxAlive = Mathf.Min(startingMaxAlive + (currentRound - 1) * maxAliveIncreasePerRound, hardMaxAlive);
        roundActive = true;

        StartCoroutine(SpawnLoop());
    }

    // -------------------------------------------------------------------------
    // Spawn loop — runs until all enemies in quota are spawned
    // -------------------------------------------------------------------------

    private IEnumerator SpawnLoop()
    {
        while (spawnedCount < totalQuota)
        {
            // Wait until there's room for another enemy
            yield return new WaitUntil(() => aliveCount < maxAlive);

            if (spawnedCount >= totalQuota) break;

            // Spawn next enemy from queue
            Vector3 spawnPos = GetSpawnPosition(spawnedCount);
            Instantiate(spawnQueue[spawnedCount], spawnPos, Quaternion.identity);
            spawnedCount++;
            aliveCount++;

            // Accelerate spawn rate over time
            currentInterval = Mathf.Max(currentInterval * spawnAcceleration, minSpawnInterval);
            yield return new WaitForSeconds(currentInterval);
        }

        roundActive = false;
    }

    // Called by EnemyBase when an enemy dies so we know a slot opened up
    public void OnEnemyDied()
    {
        aliveCount = Mathf.Max(0, aliveCount - 1);
    }

    // -------------------------------------------------------------------------
    // Spawn position
    // -------------------------------------------------------------------------

    private Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints != null && spawnPoints.Length > 0 && spawnPoints[index % spawnPoints.Length] != null)
            return spawnPoints[index % spawnPoints.Length].position;

        Camera cam = Camera.main;
        float camWidth = cam.orthographicSize * cam.aspect;

        float minX = spawnLeft != null ? spawnLeft.position.x : -10f;
        float maxX = spawnRight != null ? spawnRight.position.x : 10f;

        // Calculate both outside-camera spawn positions
        float rightSpawn = cam.transform.position.x + (camWidth + cameraEdgeOffset);
        float leftSpawn = cam.transform.position.x - (camWidth + cameraEdgeOffset);

        // Clamp to arena bounds
        rightSpawn = Mathf.Min(rightSpawn, maxX);
        leftSpawn = Mathf.Max(leftSpawn, minX);

        // If both sides are clamped to the same edge, just pick randomly within bounds
        float x;
        if (rightSpawn <= leftSpawn)
            x = Random.Range(minX, maxX);
        else
            x = (index % 2 == 0) ? rightSpawn : leftSpawn;

        return new Vector3(x, groundY + spawnHeightAboveGround, 0f);
    }

    private void Shuffle(List<GameObject> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // -------------------------------------------------------------------------
    // Editor helper
    // -------------------------------------------------------------------------

    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.red;
        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;
            Gizmos.DrawWireSphere(point.position, 0.5f);
            Gizmos.DrawLine(point.position, point.position + Vector3.up * 1.5f);
        }
    }
}