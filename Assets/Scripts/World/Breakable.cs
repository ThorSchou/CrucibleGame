using UnityEngine;

// Implements IDamageable so AttackHit can hit it through the same
// interface as enemies. HealthComponent handles HP tracking.
[RequireComponent(typeof(RecoveryCounter))]
[RequireComponent(typeof(HealthComponent))]
public class Breakable : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator animator;
    [SerializeField] private Sprite brokenSprite;
    [SerializeField] private GameObject deathParticles;
    [SerializeField] private bool destroyAfterDeath = true;
    [SerializeField] private Instantiator instantiator;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private bool requireDownAttack;
    [SerializeField] private int maxHealth = 1;

    private RecoveryCounter recoveryCounter;
    private HealthComponent health;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        recoveryCounter = GetComponent<RecoveryCounter>();
        health = GetComponent<HealthComponent>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        health.Initialize(maxHealth);
        health.OnDeath += Die;
    }

    void OnDestroy()
    {
        if (health != null) health.OnDeath -= Die;
    }

    // direction is unused by breakables but required by the interface
    public void TakeDamage(int damage, int direction)
    {
        if (health.IsDead || recoveryCounter.recovering) return;
        if (requireDownAttack && !NewPlayer.Instance.combat.pounding) return;

        if (NewPlayer.Instance.combat.pounding)
            NewPlayer.Instance.combat.PoundEffect();

        if (hitSound != null)
            GameManager.Instance.audioSource.PlayOneShot(hitSound);

        recoveryCounter.counter = 0;
        StartCoroutine(NewPlayer.Instance.combat.FreezeFrameEffect());
        animator.SetTrigger("hit");
        health.TakeDamage(damage);
    }

    private void Die()
    {
        Time.timeScale = 1;
        deathParticles.SetActive(true);
        deathParticles.transform.parent = null;

        if (instantiator != null) instantiator.InstantiateObjects();

        if (destroyAfterDeath) Destroy(gameObject);
        else spriteRenderer.sprite = brokenSprite;
    }
}