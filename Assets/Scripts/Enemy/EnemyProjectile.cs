using UnityEngine;

// Simple projectile spawned by RangedAttack. Flies in a direction, damages
// the player on contact, and self-destructs after its lifetime expires.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float arcHeight = 0f;

    [Header("Effects")]
    [SerializeField] private GameObject hitParticles;
    [SerializeField] private AudioClip hitSound;

    private Rigidbody2D rb;
    private float aliveTimer;

    // Set by RangedAttack before first frame
    [System.NonSerialized] public Vector2 direction;
    [System.NonSerialized] public int overrideDamage = -1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = arcHeight;

        int finalDamage = overrideDamage >= 0 ? overrideDamage : damage;
        damage = finalDamage;

        rb.linearVelocity = direction.normalized * speed;

        // Face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        aliveTimer += Time.deltaTime;
        if (aliveTimer >= lifetime)
            DestroySelf();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Hit the player
        if (col.TryGetComponent<NewPlayer>(out _))
        {
            if (col.TryGetComponent<IDamageable>(out var target))
            {
                int dir = transform.position.x < col.transform.position.x ? 1 : -1;
                target.TakeDamage(damage, dir);
            }
            DestroySelf();
            return;
        }

        // Blocked by player's attack hitbox
        if (col.GetComponent<AttackHit>() != null)
        {
            DestroySelf();
            return;
        }

        // Hit a wall (layer 8)
        if (col.gameObject.layer == 8)
            DestroySelf();
    }

    private void DestroySelf()
    {
        if (hitParticles != null)
            Instantiate(hitParticles, transform.position, Quaternion.identity);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

        Destroy(gameObject);
    }
}
