using System.Collections;
using UnityEngine;

// Regular dragon enemy that walks toward the player and breathes fire.
// Attach alongside EnemyBase, Walker, and RangedAttack.
// Simpler than DragonBoss — no bite attack, no rage phase.
// Works like MeleeAttacker but triggers ranged fire breath instead of melee.
public class DragonEnemy : MonoBehaviour
{
    [Header("Fire Breath")]
    [SerializeField] private float fireRange = 6f;
    [SerializeField] private float fireWindupTime = 0.4f;
    [SerializeField] private float fireCooldown = 2.5f;
    [SerializeField] private int fireballCount = 2;
    [SerializeField] private float fireSpreadAngle = 20f;
    [SerializeField] private float fireburstDelay = 0.1f;

    [Header("Follow Zones")]
    [Tooltip("If 0, uses Walker's attentionRange. Otherwise overrides the lose range.")]
    [SerializeField] private float loseRange = 0f;

    private EnemyBase enemyBase;
    private Walker walker;
    private RangedAttack rangedAttack;

    private bool isAttacking;
    private bool isFollowing;
    private float effectiveLoseRange;
    private Coroutine burstCoroutine;

    void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        walker = GetComponent<Walker>();
        rangedAttack = GetComponent<RangedAttack>();

        // Dragon controls when to fire, not RangedAttack's auto-fire
        if (rangedAttack != null)
            rangedAttack.manualOnly = true;

        effectiveLoseRange = loseRange > 0f ? loseRange : walker.attentionRange * 2f;
    }

    void Update()
    {
        if (NewPlayer.Instance.frozen) return;

        if (NewPlayer.Instance.dead)
        {
            CancelAttack();
            return;
        }

        // If hit during attack, cancel and let knockback play out
        if (isAttacking && enemyBase.recoveryCounter.recovering)
        {
            CancelAttack();
            return;
        }

        float dist = Vector2.Distance(transform.position, NewPlayer.Instance.transform.position);

        UpdateFollowState(dist);

        if (!isAttacking && isFollowing && dist <= fireRange)
            StartCoroutine(FireBreathAttack());
    }

    private void UpdateFollowState(float dist)
    {
        if (!isFollowing && dist <= walker.attentionRange)
            isFollowing = true;
        else if (isFollowing && dist > effectiveLoseRange)
            isFollowing = false;

        walker.neverStopFollowing = isFollowing;
    }

    private IEnumerator FireBreathAttack()
    {
        isAttacking = true;

        // Stop moving and face the player
        walker.sitStillWhenAttacking = true;
        FacePlayer();

        // Fire breath animation
        if (enemyBase.animator != null)
            enemyBase.animator.SetTrigger("fireBreath");

        // Wind-up
        yield return new WaitForSeconds(fireWindupTime);

        // Fire projectiles (unless interrupted by knockback)
        if (rangedAttack != null && !enemyBase.recoveryCounter.recovering)
        {
            FacePlayer(); // re-aim in case player moved during windup
            if (fireballCount > 1)
                burstCoroutine = rangedAttack.FireBurst(fireballCount, fireSpreadAngle, fireburstDelay);
            else
                rangedAttack.Fire();
        }

        // Wait for burst to finish
        float burstTime = fireballCount > 1 ? (fireballCount - 1) * fireburstDelay : 0f;
        yield return new WaitForSeconds(burstTime + 0.1f);

        // Cooldown — scales with attack speed so it gets faster in later rounds
        float cd = fireCooldown / enemyBase.AttackSpeed;
        yield return new WaitForSeconds(cd);

        // Resume movement
        walker.sitStillWhenAttacking = false;
        isAttacking = false;
    }

    private void CancelAttack()
    {
        if (!isAttacking) return;
        StopAllCoroutines();
        if (burstCoroutine != null && rangedAttack != null)
        {
            rangedAttack.StopCoroutine(burstCoroutine);
            burstCoroutine = null;
        }
        walker.sitStillWhenAttacking = false;
        isAttacking = false;
    }

    private void FacePlayer()
    {
        float dx = NewPlayer.Instance.transform.position.x - transform.position.x;
        if (walker != null)
            walker.direction = dx < 0 ? -1 : 1;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f); // orange
        Gizmos.DrawWireSphere(transform.position, fireRange);
    }
}
