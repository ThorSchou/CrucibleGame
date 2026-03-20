using System.Collections;
using UnityEngine;

// Handles melee attack behaviour for enemies that swing a weapon.
// Attach alongside EnemyBase and Walker.
// The weapon hitbox child should have an AttackHit component, disabled by default.
public class MeleeAttacker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject weaponHitbox;   // child GO with AttackHit, disabled by default

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;  // distance to trigger swing
    [SerializeField] private float hitboxDelay = 0.15f; // wind-up before hitbox activates
    [SerializeField] private float hitboxDuration = 0.2f;  // how long hitbox stays active

    [Header("Stop/Follow Zones")]
    // Notice zone is Walker's attentionRange
    // Lose zone is 2x that — set automatically below
    private float loseRange;

    private EnemyBase enemyBase;
    private Walker walker;
    private bool isAttacking = false;
    private bool isFollowing = false;

    void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        walker = GetComponent<Walker>();

        // Lose range is 2x the notice range
        loseRange = walker.attentionRange * 2f;

        // Disable weapon hitbox by default
        if (weaponHitbox != null) weaponHitbox.SetActive(false);
    }

    void Update()
    {
        if (enemyBase == null || NewPlayer.Instance == null) return;

        if (NewPlayer.Instance.dead)
        {
            CancelAttack();
            return;
        }
        // Safety — if enemy is being knocked back, cancel attack
        if (isAttacking && enemyBase.recoveryCounter.recovering)
        {
            CancelAttack();
            return;
        }

        float distanceToPlayer = Vector2.Distance(
            transform.position,
            NewPlayer.Instance.transform.position);

        UpdateFollowState(distanceToPlayer);

        if (!isAttacking && isFollowing && distanceToPlayer <= attackRange)
            StartCoroutine(Attack());
    }

    private void UpdateFollowState(float distanceToPlayer)
    {
        if (!isFollowing && distanceToPlayer <= walker.attentionRange)
            isFollowing = true;
        else if (isFollowing && distanceToPlayer > loseRange)
            isFollowing = false;

        // Tell Walker to chase or wander
        walker.neverStopFollowing = false;
        enemyBase.isChasing = isFollowing;

        // Freeze Walker movement during attack
        walker.sitStillWhenAttacking = isAttacking;
    }

    private IEnumerator Attack()
    {
        isAttacking = true;

        // Trigger animation and freeze movement
        enemyBase.animator.SetTrigger("attack");

        // Wind-up delay before hitbox activates
        yield return new WaitForSeconds(hitboxDelay);

        // Activate hitbox
        if (weaponHitbox != null)
        {
            weaponHitbox.GetComponent<AttackHit>().hitTargets.Clear();
            weaponHitbox.SetActive(true);
        }

        // Freeze frame effect like player attack
        yield return StartCoroutine(NewPlayer.Instance.combat.FreezeFrameEffect());

        // Keep hitbox active for duration
        yield return new WaitForSeconds(hitboxDuration);

        // Deactivate hitbox
        if (weaponHitbox != null) weaponHitbox.SetActive(false);

        // Wait for attack animation to finish before allowing next attack
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = enemyBase.animator.GetCurrentAnimatorStateInfo(0);
            return !state.IsName("Attack");
        });

        isAttacking = false;
    }

    public void CancelAttack()
    {
        if (!isAttacking) return;
        StopAllCoroutines();
        if (weaponHitbox != null) weaponHitbox.SetActive(false);
        if (walker != null) walker.sitStillWhenAttacking = false;
        isAttacking = false;
    }
}