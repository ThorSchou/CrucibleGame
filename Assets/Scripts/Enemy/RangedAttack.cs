using System.Collections;
using UnityEngine;

// Add to any enemy that should shoot projectiles at the player.
// Works with both Walkers and Flyers. Spawns EnemyProjectile prefabs
// at a configurable fire point with adjustable rate and range.
public class RangedAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private EnemyBase enemyBase;

    [Header("Settings")]
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float minAttackRange = 0f;
    [SerializeField] private bool useArc = false;
    [SerializeField] private float arcAngle = 30f;

    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;

    /// When true, Update() won't auto-fire. Use Fire() or FireBurst() from external scripts.
    [System.NonSerialized] public bool manualOnly;

    private float fireCooldown;
    private AudioSource audioSource;

    void Start()
    {
        if (enemyBase == null)
            enemyBase = GetComponent<EnemyBase>();
        audioSource = GetComponent<AudioSource>();
        fireCooldown = fireRate;
    }

    void Update()
    {
        if (manualOnly) return;
        if (NewPlayer.Instance.frozen) return;

        float dist = Vector2.Distance(transform.position, NewPlayer.Instance.transform.position);
        bool inRange = dist <= attackRange && dist >= minAttackRange;

        if (!inRange) return;

        // Scale fire rate with attack speed from EnemyBase
        float effectiveRate = fireRate / enemyBase.AttackSpeed;
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = effectiveRate;
        }
    }

    public void Fire()
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 toPlayer = (NewPlayer.Instance.transform.position - spawnPos);

        Vector2 dir;
        if (useArc)
        {
            // Lob upward at an angle toward the player
            float sign = Mathf.Sign(toPlayer.x);
            float rad = arcAngle * Mathf.Deg2Rad;
            dir = new Vector2(sign * Mathf.Cos(rad), Mathf.Sin(rad));
        }
        else
        {
            dir = toPlayer.normalized;
        }

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        if (proj.TryGetComponent<EnemyProjectile>(out var ep))
        {
            ep.direction = dir;
            ep.overrideDamage = enemyBase.AttackDamage;
        }

        if (audioSource != null && fireSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(fireSound);
        }
    }

    /// Fires multiple projectiles in a spread pattern over time.
    /// Called externally by boss scripts like DragonBoss.
    public Coroutine FireBurst(int count, float spreadAngle, float delayBetween = 0.1f)
    {
        return StartCoroutine(FireBurstRoutine(count, spreadAngle, delayBetween));
    }

    private IEnumerator FireBurstRoutine(int count, float spreadAngle, float delayBetween)
    {
        if (projectilePrefab == null) yield break;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 toPlayer = (NewPlayer.Instance.transform.position - spawnPos).normalized;

        float startAngle = -spreadAngle / 2f;
        float step = count > 1 ? spreadAngle / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;
            float rad = angle * Mathf.Deg2Rad;
            // Rotate base direction by angle
            Vector2 dir = new Vector2(
                toPlayer.x * Mathf.Cos(rad) - toPlayer.y * Mathf.Sin(rad),
                toPlayer.x * Mathf.Sin(rad) + toPlayer.y * Mathf.Cos(rad));

            Vector3 pos = firePoint != null ? firePoint.position : transform.position;
            GameObject proj = Instantiate(projectilePrefab, pos, Quaternion.identity);
            if (proj.TryGetComponent<EnemyProjectile>(out var ep))
            {
                ep.direction = dir;
                ep.overrideDamage = enemyBase.AttackDamage;
            }

            if (audioSource != null && fireSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(fireSound);
            }

            if (i < count - 1)
                yield return new WaitForSeconds(delayBetween);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (minAttackRange > 0f)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, minAttackRange);
        }
    }
}
