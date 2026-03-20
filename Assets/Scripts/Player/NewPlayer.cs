using UnityEngine;
using UnityEngine.InputSystem;

// Responsible for: reading input, moving the character, jumping, freezing.
// Combat, health, and death all live in PlayerCombat.
[RequireComponent(typeof(RecoveryCounter))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(WeaponComponent))]
public class NewPlayer : PhysicsObject
{
    [Header("References")]
    public AudioSource audioSource;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject graphic;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private GameObject pauseMenu;
    public CameraEffects cameraEffects;
    public RecoveryCounter recoveryCounter;

    // Filled in Start — other scripts access combat and stats through these
    [System.NonSerialized] public PlayerCombat combat;
    [System.NonSerialized] public PlayerStats stats;

    // Singleton
    private static NewPlayer instance;
    public static NewPlayer Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<NewPlayer>();
            return instance;
        }
    }

    [Header("Input Actions")]
    // Assign from your Input Action Asset in the Inspector
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference poundAction;
    [SerializeField] private InputActionReference pauseAction;

    [Header("Properties")]
    // Assign items here in the Inspector during development to start with them immediately
    [SerializeField] private string[] cheatItems;
    public bool dead = false;
    public bool frozen = false;

    [System.NonSerialized] public float launch = 0f;
    [System.NonSerialized] public float jumpPower;
    [System.NonSerialized] public string groundType = "grass";
    [System.NonSerialized] public RaycastHit2D ground;

    [SerializeField] private float launchRecovery = 5f;
    [SerializeField] private float fallForgiveness = .2f;

    private bool jumping;
    private Vector2 moveInput;
    private Vector3 origLocalScale;
    private float fallForgivenessCounter;
    private bool wasGrounded;

    [Header("Sounds")]
    public AudioClip grassSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip stepSound;


    private void OnEnable()
    {
        jumpAction.action.performed += OnJump;
        attackAction.action.performed += OnAttack;
        poundAction.action.performed += OnPound;
        pauseAction.action.performed += OnPause;

        // Remove these five lines if using a PlayerInput component on this GameObject
        moveAction.action.Enable();
        jumpAction.action.Enable();
        attackAction.action.Enable();
        poundAction.action.Enable();
        pauseAction.action.Enable();
    }

    private void OnDisable()
    {
        jumpAction.action.performed -= OnJump;
        attackAction.action.performed -= OnAttack;
        poundAction.action.performed -= OnPound;
        pauseAction.action.performed -= OnPause;
    }

    void Start()
    {
        Cursor.visible = false;
        combat = GetComponent<PlayerCombat>();
        stats = GetComponent<PlayerStats>();
        recoveryCounter = GetComponent<RecoveryCounter>();
        origLocalScale = transform.localScale;
        jumpPower = stats.JumpPower;

        SetGroundType();
        SetUpCheatItems();
    }

    private void Update()
    {
        ComputeVelocity();
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    protected void ComputeVelocity()
    {
        if (dead) return;
        Vector2 move = Vector2.zero;
        ground = Physics2D.Raycast(transform.position, -Vector2.up);

        launch += (0 - launch) * Time.deltaTime * launchRecovery;

        if (!frozen)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            move.x = moveInput.x + launch;

            if (moveInput.x > 0.01f)
                graphic.transform.localScale = new Vector3(origLocalScale.x, transform.localScale.y, transform.localScale.z);
            else if (moveInput.x < -0.01f)
                graphic.transform.localScale = new Vector3(-origLocalScale.x, transform.localScale.y, transform.localScale.z);

            if (!grounded)
            {
                if (fallForgivenessCounter < fallForgiveness && !jumping)
                    fallForgivenessCounter += Time.deltaTime;
                else
                    animator.SetBool("grounded", false);
            }
            else
            {
                // Player just landed this frame
                if (!wasGrounded)
                {
                    LandEffect();
                    velocity.y = 0;
                }
                fallForgivenessCounter = 0;
                animator.SetBool("grounded", true);
            }

            wasGrounded = grounded;

            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / stats.MoveSpeed);
            animator.SetFloat("velocityY", velocity.y);
            animator.SetInteger("attackDirectionY", (int)moveInput.y);
            animator.SetInteger("moveDirection", (int)moveInput.x);

            targetVelocity = move * stats.MoveSpeed;
        }
        else
        {
            launch = 0;
            moveInput = Vector2.zero;
        }
    }

    // -------------------------------------------------------------------------
    // Input callbacks
    // -------------------------------------------------------------------------

    private void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!frozen && animator.GetBool("grounded") && !jumping)
        {
            animator.SetBool("pounded", false);
            Jump(1f);
        }
    }

    private void OnAttack(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!frozen && Time.timeScale > 0) combat.OnAttackInput();
    }

    private void OnPound(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!frozen && !grounded) combat.ActivatePound();
    }

    private void OnPause(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (pauseMenu.activeSelf)
            pauseMenu.GetComponent<PauseMenu>().Unpause();
        else
            pauseMenu.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Public methods
    // -------------------------------------------------------------------------

    public void Jump(float jumpMultiplier)
    {
        
        if (velocity.y != jumpPower)
        {
            velocity.y = jumpPower * jumpMultiplier;
            PlayJumpSound();
            PlayStepSound();
            JumpEffect();
            jumping = true;
        }
    }

    public void Freeze(bool freeze)
    {
        if (freeze)
        {
            animator.SetInteger("moveDirection", 0);
            animator.SetBool("grounded", true);
            animator.SetFloat("velocityX", 0f);
            animator.SetFloat("velocityY", 0f);
            targetVelocity = Vector2.zero;
        }
        frozen = freeze;
        launch = 0;
    }

    public void ResetLevel()
    {
        Freeze(true);
        dead = false;
        GetComponent<HealthComponent>().ResetHealth();
    }

    public void SetGroundType()
    {
        switch (groundType)
        {
            case "Grass":
                stepSound = grassSound;
                break;
        }
    }

    private void SetUpCheatItems()
    {
        foreach (string item in cheatItems)
            GameManager.Instance.GetInventoryItem(item, null);
    }

    // -------------------------------------------------------------------------
    // Movement sounds and effects
    // -------------------------------------------------------------------------

    public void PlayStepSound()
    {
        if (stepSound == null) return;
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(stepSound, Mathf.Abs(moveInput.x / 10));
    }

    public void PlayJumpSound()
    {
        if (jumpSound == null) return;
        audioSource.pitch = 1f;
        GameManager.Instance.audioSource.PlayOneShot(jumpSound, .1f);
    }

    public void JumpEffect()
    {
        if (jumpParticles != null) jumpParticles.Emit(1);
        if (landSound == null) return;
        audioSource.pitch = Random.Range(0.6f, 1f);
        audioSource.PlayOneShot(landSound);
    }

    public void LandEffect()
    {
        if (!jumping) return;
        if (jumpParticles != null) jumpParticles.Emit(1);
        if (landSound != null)
        {
            audioSource.pitch = Random.Range(0.6f, 1f);
            audioSource.PlayOneShot(landSound);
        }
        jumping = false;
    }
}