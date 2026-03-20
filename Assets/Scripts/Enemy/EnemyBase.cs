using UnityEngine;

[RequireComponent(typeof(RecoveryCounter))]
[RequireComponent(typeof(HealthComponent))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("References")]
    [System.NonSerialized] public AudioSource audioSource;
    public Animator animator;
    [SerializeField] private Instantiator instantiator;
    [System.NonSerialized] public RecoveryCounter recoveryCounter;

    [Header("Properties")]
    [SerializeField] private GameObject deathParticles;
    public AudioClip hitSound;
    public bool isBomb;
    [SerializeField] private bool requirePoundAttack;

    [Header("Base Stats")]
    [SerializeField] private int baseHealth = 3;
    [SerializeField] private float baseMovementSpeed = 7f;
    [SerializeField] private int baseAttackDamage = 1;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseJumpHeight = 7f;
    [SerializeField] private float baseJumpSpeed = 7f;

    [Header("Speed Variance")]
    [SerializeField] private float speedVarianceMin = 0.85f;
    [SerializeField] private float speedVarianceMax = 1.15f;
    private float speedVarianceMultiplier = 1f;

    [Header("Stamina")]
    [SerializeField] private bool useStamina = false;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 10f;
    [SerializeField] private float staminaRecoveryRate = 15f;
    [Tooltip("Lowest stat multiplier when fully exhausted (0 = no stats, 1 = no penalty)")]
    [SerializeField] private float staminaMinMultiplier = 0.3f;

    private float currentStamina;

    [System.NonSerialized] public bool isChasing;

    public float StaminaNormalized => maxStamina > 0f ? currentStamina / maxStamina : 1f;
    public float StaminaMultiplier => useStamina
        ? Mathf.Lerp(staminaMinMultiplier, 1f, StaminaNormalized)
        : 1f;

    public float MovementSpeed => baseMovementSpeed * RoundManager.Instance.SpeedMultiplier * StaminaMultiplier * speedVarianceMultiplier;
    public int AttackDamage => Mathf.Max(1, Mathf.RoundToInt(baseAttackDamage * RoundManager.Instance.DamageMultiplier));
    public float AttackSpeed => baseAttackSpeed * StaminaMultiplier;
    public float JumpHeight => baseJumpHeight * StaminaMultiplier;
    public float JumpSpeed => baseJumpSpeed * StaminaMultiplier;

    private HealthComponent health;
    private bool isDead = false;

    private Walker walker;
    private Flyer flyer;
    private AttackHit[] attackHits;
    private MeleeAttacker meleeAttacker;


    void Awake()
    {
        RoundManager.Instance.RegisterEnemy();
        speedVarianceMultiplier = Random.Range(speedVarianceMin, speedVarianceMax);
    }
    void Start()
    {
        recoveryCounter = GetComponent<RecoveryCounter>();
        audioSource = GetComponent<AudioSource>();
        health = GetComponent<HealthComponent>();
        walker = GetComponent<Walker>();
        flyer = GetComponent<Flyer>();
        meleeAttacker = GetComponent<MeleeAttacker>();
        attackHits = GetComponentsInChildren<AttackHit>();

        currentStamina = maxStamina;

        int scaledHealth = Mathf.RoundToInt(baseHealth * RoundManager.Instance.HealthMultiplier);
        health.Initialize(scaledHealth);
        health.OnDeath += Die;
    }

    void Update()
    {
        if (useStamina) UpdateStamina();
        SyncAttackDamage();
    }

    private void UpdateStamina()
    {
        if (isChasing)
            currentStamina = Mathf.Max(0f, currentStamina - staminaDrainRate * Time.deltaTime);
        else
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRecoveryRate * Time.deltaTime);
    }

    private void SyncAttackDamage()
    {
        int dmg = AttackDamage;
        for (int i = 0; i < attackHits.Length; i++)
            attackHits[i].SetHitPower(dmg);
    }

    void OnDestroy()
    {
        if (health != null) health.OnDeath -= Die;
    }

    public void TakeDamage(int damage, int direction)
    {
        if (isDead || recoveryCounter.recovering) return;
        if (requirePoundAttack && !NewPlayer.Instance.combat.pounding) return;

        // Cancel any active melee attack so knockback applies cleanly
        if (meleeAttacker != null) meleeAttacker.CancelAttack();

        NewPlayer.Instance.cameraEffects.Shake(100, 1);
        if (animator != null) animator.SetTrigger("hurt");
        if (audioSource != null && hitSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(hitSound);
        }

        recoveryCounter.counter = 0;
        recoveryCounter.recovering = true;

        if (NewPlayer.Instance.combat.pounding)
            NewPlayer.Instance.combat.PoundEffect();

        ApplyKnockback(direction);
        StartCoroutine(NewPlayer.Instance.combat.FreezeFrameEffect());
        health.TakeDamage(damage);
    }

    private void ApplyKnockback(int direction)
    {
        if (walker != null)
        {
            walker.launch = direction * walker.hurtLaunchPower / 5;
            walker.velocity.y = walker.hurtLaunchPower;
        }
        else if (flyer != null)
        {
            flyer.speedEased.x = direction * 5;
            flyer.speedEased.y = 4;
            flyer.speed.x = flyer.speedEased.x;
            flyer.speed.y = flyer.speedEased.y;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (NewPlayer.Instance.combat.pounding)
            NewPlayer.Instance.combat.PoundEffect();

        NewPlayer.Instance.cameraEffects.Shake(200, 1);

        if (deathParticles != null)
        {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = transform.parent;
        }

        if (instantiator != null)
            instantiator.InstantiateObjects();

        Time.timeScale = 1f;
        RoundManager.Instance.UnregisterEnemy(transform.position);
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null) spawner.OnEnemyDied();
        GameManager.Instance.RegisterEnemyKill();
        Destroy(gameObject);
    }
}