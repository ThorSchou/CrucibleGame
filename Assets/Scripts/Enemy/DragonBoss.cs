using UnityEngine;

// Dragon boss AI state machine. Sits alongside EnemyBase, Walker, and RangedAttack.
// Manages attack decisions: fire breath at range, bite up close.
// Walker handles all ground movement; RangedAttack handles projectile spawning.
public class DragonBoss : MonoBehaviour
{
    private enum State { Patrol, Chase, FireBreath, Bite, Cooldown }

    [Header("Attack Ranges")]
    [SerializeField] private float biteRange = 1.5f;
    [SerializeField] private float fireRange = 7f;

    [Header("Fire Breath")]
    [SerializeField] private int fireballCount = 3;
    [SerializeField] private float fireSpreadAngle = 30f;
    [SerializeField] private float fireburstDelay = 0.12f;
    [SerializeField] private float fireWindupTime = 0.5f;

    [Header("Bite")]
    [SerializeField] private float biteWindupTime = 0.3f;
    [SerializeField] private float biteDuration = 0.4f;
    [SerializeField] private GameObject biteHitbox;

    [Header("Cooldown")]
    [SerializeField] private float baseCooldown = 1.5f;

    [Header("Rage Phase")]
    [SerializeField] private float rageHealthThreshold = 0.5f;
    [SerializeField] private int rageFireballCount = 5;
    [SerializeField] private float rageCooldownMultiplier = 0.6f;

    private State currentState = State.Patrol;
    private float stateTimer;
    private Coroutine burstCoroutine;

    private EnemyBase enemyBase;
    private Walker walker;
    private RangedAttack rangedAttack;
    private HealthComponent health;

    private bool IsRaging => health != null && health.MaxHealth > 0
        && (float)health.CurrentHealth / health.MaxHealth < rageHealthThreshold;

    void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        walker = GetComponent<Walker>();
        rangedAttack = GetComponent<RangedAttack>();
        health = GetComponent<HealthComponent>();

        // Dragon controls when to fire, not RangedAttack's auto-fire
        if (rangedAttack != null)
            rangedAttack.manualOnly = true;

        // Start with bite hitbox disabled
        if (biteHitbox != null)
            biteHitbox.SetActive(false);
    }

    void Update()
    {
        if (NewPlayer.Instance.frozen) return;
        if (enemyBase.recoveryCounter.recovering)
        {
            // Got hit — interrupt attacks and go to cooldown
            if (currentState == State.FireBreath || currentState == State.Bite)
            {
                CancelAttack();
                EnterState(State.Cooldown);
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, NewPlayer.Instance.transform.position);

        switch (currentState)
        {
            case State.Patrol:
                // Walker handles patrol movement. Switch to chase when player is near.
                if (dist < walker.attentionRange)
                    EnterState(State.Chase);
                break;

            case State.Chase:
                // Walker handles chasing. Pick an attack when in range.
                if (dist <= biteRange)
                    EnterState(State.Bite);
                else if (dist <= fireRange)
                    EnterState(State.FireBreath);
                break;

            case State.FireBreath:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    EnterState(State.Cooldown);
                break;

            case State.Bite:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    EnterState(State.Cooldown);
                break;

            case State.Cooldown:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    EnterState(State.Chase);
                break;
        }
    }

    private void EnterState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Chase:
                EnableMovement(true);
                break;

            case State.FireBreath:
                EnableMovement(false);
                FacePlayer();
                int count = IsRaging ? rageFireballCount : fireballCount;
                float totalTime = fireWindupTime + count * fireburstDelay;
                stateTimer = totalTime;
                if (enemyBase.animator != null)
                    enemyBase.animator.SetTrigger("fireBreath");
                // Fire after windup
                Invoke(nameof(DoFireBurst), fireWindupTime);
                break;

            case State.Bite:
                EnableMovement(false);
                FacePlayer();
                stateTimer = biteWindupTime + biteDuration;
                if (enemyBase.animator != null)
                    enemyBase.animator.SetTrigger("bite");
                Invoke(nameof(ActivateBiteHitbox), biteWindupTime);
                Invoke(nameof(DeactivateBiteHitbox), biteWindupTime + biteDuration);
                break;

            case State.Cooldown:
                EnableMovement(false);
                float cd = baseCooldown / enemyBase.AttackSpeed;
                if (IsRaging) cd *= rageCooldownMultiplier;
                stateTimer = cd;
                break;

            case State.Patrol:
                EnableMovement(true);
                break;
        }
    }

    private void DoFireBurst()
    {
        if (rangedAttack == null || currentState != State.FireBreath) return;
        int count = IsRaging ? rageFireballCount : fireballCount;
        burstCoroutine = rangedAttack.FireBurst(count, fireSpreadAngle, fireburstDelay);
    }

    private void ActivateBiteHitbox()
    {
        if (biteHitbox != null && currentState == State.Bite)
            biteHitbox.SetActive(true);
    }

    private void DeactivateBiteHitbox()
    {
        if (biteHitbox != null)
            biteHitbox.SetActive(false);
    }

    private void CancelAttack()
    {
        CancelInvoke();
        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            burstCoroutine = null;
        }
        DeactivateBiteHitbox();
    }

    private void EnableMovement(bool enable)
    {
        // Use sitStillWhenAttacking so gravity/knockback still work while stopped
        if (walker != null)
            walker.sitStillWhenAttacking = !enable;
    }

    private void FacePlayer()
    {
        float dx = NewPlayer.Instance.transform.position.x - transform.position.x;
        if (walker != null)
            walker.direction = dx < 0 ? -1 : 1;

        // Apply flip immediately
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(
            Mathf.Abs(s.x) * (dx < 0 ? -1 : 1), s.y, s.z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, biteRange);
    }
}
