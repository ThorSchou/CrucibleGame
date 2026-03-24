using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Responsible for: taking damage, attacking, pounding, death, and all
// combat visual/audio effects. Implements IDamageable so AttackHit can
// hit the player through the same interface it uses for enemies.
[RequireComponent(typeof(NewPlayer))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(RecoveryCounter))]
public class PlayerCombat : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem deathParticles;

    [Header("Hurt Settings")]
    [SerializeField] private Vector2 hurtLaunchPower;

    [Header("Pound")]
    public bool pounding = false;

    [Header("Attack")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackDuration = 0.2f;

    [Header("Sounds")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField] private AudioClip poundSound;
    [SerializeField] private AudioClip punchSound;
    [SerializeField] private AudioClip[] poundActivationSounds;

    private NewPlayer player;
    private HealthComponent health;
    private RecoveryCounter recoveryCounter;
    private Component[] graphicSprites;
    private int whichHurtSound = 0;
    private bool isAttacking = false;

    void Start()
    {
        player = GetComponent<NewPlayer>();
        health = GetComponent<HealthComponent>();
        recoveryCounter = GetComponent<RecoveryCounter>();
        graphicSprites = GetComponentsInChildren<SpriteRenderer>();

        health.Initialize(GetComponent<PlayerStats>().MaxHealth);
        health.OnDeath += () => StartCoroutine(Die());
    }

    // -------------------------------------------------------------------------
    // IDamageable — called by AttackHit on enemy attacks hitting the player
    // -------------------------------------------------------------------------

    public void TakeDamage(int damage, int direction)
    {
        if (player.dead) return;
        if (player.frozen || recoveryCounter.recovering || pounding) return;

        // Cancel attack if hit during one
        if (isAttacking)
        {
            StopCoroutine(nameof(ActivateHitbox));
            attackHitbox.SetActive(false);
            isAttacking = false;
            player.Freeze(false);
        }

        HurtEffect();
        player.cameraEffects.Shake(100, 1);
        animator.SetTrigger("hurt");

        player.velocity.y = hurtLaunchPower.y;
        player.launch = direction * hurtLaunchPower.x;

        recoveryCounter.counter = 0;
        health.TakeDamage(damage);

        GameManager.Instance.hud.HealthBarHurt();
    }

    private void HurtEffect()
    {
        if (hurtSound != null)
            GameManager.Instance.audioSource.PlayOneShot(hurtSound);

        if (hurtSounds != null && hurtSounds.Length > 0)
        {
            GameManager.Instance.audioSource.PlayOneShot(hurtSounds[whichHurtSound]);
            whichHurtSound = whichHurtSound >= hurtSounds.Length - 1 ? 0 : whichHurtSound + 1;
        }

        StartCoroutine(FreezeFrameEffect());
        player.cameraEffects.Shake(100, 1f);
    }

    public IEnumerator FreezeFrameEffect(float length = .007f)
    {
        Time.timeScale = .1f;
        yield return new WaitForSeconds(length);
        Time.timeScale = 1f;
    }

    private IEnumerator Die()
    {
        player.dead = true;
        player.Freeze(false);
        Time.timeScale = 1f;

        if (deathParticles != null) deathParticles.Emit(10);
        if (deathSound != null) GameManager.Instance.audioSource.PlayOneShot(deathSound);

        Time.timeScale = 0.3f;
        animator.SetTrigger("death");

        yield return new WaitForSecondsRealtime(2f);

        animator.enabled = false;
        player.Freeze(true);
        gameObject.layer = LayerMask.NameToLayer("Dead");
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        DeathScreen deathScreen = FindFirstObjectByType<DeathScreen>(FindObjectsInactive.Include);
        if (deathScreen != null) deathScreen.Show();
        else Debug.LogError("DeathScreen not found!");
    }

    // -------------------------------------------------------------------------
    // Combat actions — called by NewPlayer input callbacks
    // -------------------------------------------------------------------------

    public void OnAttackInput()
    {
        if (isAttacking) return;
        animator.SetTrigger("attack");
        PunchEffect();
        if (attackHitbox != null)
            StartCoroutine(nameof(ActivateHitbox));
    }

    private IEnumerator ActivateHitbox()
    {
        isAttacking = true;
        player.Freeze(true);
        player.launch = 0;

        attackHitbox.GetComponent<AttackHit>().hitTargets.Clear();
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(attackDuration);
        attackHitbox.SetActive(false);

        player.Freeze(false);
        isAttacking = false;
    }

    public void ActivatePound()
    {
        if (pounding) return;

        animator.SetBool("pounded", false);

        if (player.velocity.y <= 0)
            player.velocity = new Vector2(player.velocity.x, hurtLaunchPower.y / 2);

        if (poundActivationSounds != null && poundActivationSounds.Length > 0)
            GameManager.Instance.audioSource.PlayOneShot(
                poundActivationSounds[Random.Range(0, poundActivationSounds.Length)]);

        pounding = true;
        StartCoroutine(FreezeFrameEffect(.3f));
    }

    public void PoundEffect()
    {
        if (!pounding) return;

        animator.ResetTrigger("attack");
        player.velocity.y = player.jumpPower / 1.4f;
        animator.SetBool("pounded", true);
        GameManager.Instance.audioSource.PlayOneShot(poundSound);
        player.cameraEffects.Shake(200, 1f);
        pounding = false;
        recoveryCounter.counter = 0;
    }

    public void PunchEffect()
    {
        if (punchSound != null)
            GameManager.Instance.audioSource.PlayOneShot(punchSound);
        player.cameraEffects.Shake(100, 1f);
    }

    public void FlashEffect()
    {
        animator.SetTrigger("flash");
    }

    public void Hide(bool hide)
    {
        player.Freeze(hide);
        foreach (SpriteRenderer sprite in graphicSprites)
            sprite.gameObject.SetActive(!hide);
    }

    public void RestartPlayer()
    {
        player.dead = false;
        player.Freeze(false);
        gameObject.layer = LayerMask.NameToLayer("Player");
        GetComponent<Collider2D>().enabled = true;
        GetComponent<HealthComponent>().ResetHealth();
        if (animator != null) animator.enabled = true;
        if (isAttacking)
        {
            StopCoroutine(nameof(ActivateHitbox));
            attackHitbox.SetActive(false);
            isAttacking = false;
        }
    }
}