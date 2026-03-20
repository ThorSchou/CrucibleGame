using UnityEngine;

public class Walker : PhysicsObject
{
    [Header("Reference")]
    public EnemyBase enemyBase;
    [SerializeField] private GameObject graphic;

    [Header("Properties")]
    [SerializeField] private LayerMask layerMask;

    private enum EnemyType { Bug, Zombie }
    [SerializeField] private EnemyType enemyType;

    public float attentionRange;
    public float changeDirectionEase = 1;
    public float hurtLaunchPower = 10;
    [SerializeField] private bool canJumpOverWalls = false;
    public bool neverStopFollowing = false;
    [SerializeField] private bool sitStillWhenNotFollowing = false;

    [Header("Flee Behavior")]
    [Tooltip("If > 0, enemy runs away when player is closer than this distance.")]
    [SerializeField] private float fleeRange = 0f;

    [SerializeField] private Vector2 rayCastSize = new Vector2(1.5f, 1);
    [SerializeField] private Vector2 rayCastOffset;

    [System.NonSerialized] public float direction = 1;
    [System.NonSerialized] public float directionSmooth = 1;
    [System.NonSerialized] public float launch = 0;
    [System.NonSerialized] public bool sitStillWhenAttacking = false;

    private Vector2 distanceFromPlayer;
    private RaycastHit2D ground;
    private RaycastHit2D rightWall;
    private RaycastHit2D leftWall;
    private RaycastHit2D rightLedge;
    private RaycastHit2D leftLedge;
    private Vector3 origScale;
    private Vector2 rayCastSizeOrig;
    private float sitStillMultiplier = 1;
    private bool isFollowingPlayer = false;

    [Header("Sounds")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip stepSound;

    void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        origScale = transform.localScale;
        rayCastSizeOrig = rayCastSize;
        launch = 0;

        if (enemyType == EnemyType.Zombie)
        {
            direction = 0;
            directionSmooth = 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attentionRange);
    }

    private void Update()
    {
        ComputeVelocity();
    }

    protected void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;
        float currentMaxSpeed = enemyBase.MovementSpeed;

        distanceFromPlayer = new Vector2(
            NewPlayer.Instance.transform.position.x - transform.position.x,
            NewPlayer.Instance.transform.position.y - transform.position.y);

        // If player is dead, wander and ignore player
        if (NewPlayer.Instance.dead)
        {
            isFollowingPlayer = false;
            enemyBase.isChasing = false;
            rayCastSize.y = rayCastSizeOrig.y;
        }

        // Freeze movement during attack but still apply launch decay
        if (!sitStillWhenAttacking)
        {
            directionSmooth += ((direction * sitStillMultiplier) - directionSmooth)
                               * Time.deltaTime * changeDirectionEase;
            if (Mathf.Abs(launch) > 0.5f)
                move.x = launch;
            else
                move.x = directionSmooth + launch;
        }
        launch += (0 - launch) * Time.deltaTime;

        // Flip based on direction, not directionSmooth
        if (direction < 0)
            transform.localScale = new Vector3(-origScale.x, origScale.y, origScale.z);
        else
            transform.localScale = new Vector3(origScale.x, origScale.y, origScale.z);

        if (!enemyBase.recoveryCounter.recovering && Mathf.Abs(launch) < 0.5f)
        {
            if (!NewPlayer.Instance.dead)
            {
                if (enemyType == EnemyType.Zombie)
                {
                    bool playerInRange = Mathf.Abs(distanceFromPlayer.x) < attentionRange
                                      && Mathf.Abs(distanceFromPlayer.y) < attentionRange;

                    if (playerInRange)
                    {
                        isFollowingPlayer = true;
                        sitStillMultiplier = 1;
                        if (neverStopFollowing)
                            attentionRange = float.MaxValue;
                    }
                    else
                    {
                        sitStillMultiplier = sitStillWhenNotFollowing ? 0 : 1;
                    }
                }

                enemyBase.isChasing = isFollowingPlayer || enemyType == EnemyType.Bug;

                if (isFollowingPlayer || enemyType == EnemyType.Bug)
                {
                    if (isFollowingPlayer)
                    {
                        rayCastSize.y = 200;
                        direction = distanceFromPlayer.x < 0 ? -1 : 1;
                        if (fleeRange > 0f && distanceFromPlayer.magnitude < fleeRange)
                            direction = -direction;
                    }
                    else
                    {
                        rayCastSize.y = rayCastSizeOrig.y;
                    }
                }
            }
            else
            {
                // Player is dead — stop following and wander
                isFollowingPlayer = false;
                enemyBase.isChasing = false;
                rayCastSize.y = rayCastSizeOrig.y;
            }

            // Wall detection
            rightWall = Physics2D.Raycast(
                new Vector2(transform.position.x, transform.position.y + rayCastOffset.y),
                Vector2.right, rayCastSize.x, layerMask);
            Debug.DrawRay(
                new Vector2(transform.position.x, transform.position.y + rayCastOffset.y),
                Vector2.right * rayCastSize.x, Color.yellow);

            if (rightWall.collider != null)
            {
                if (!isFollowingPlayer) direction = -1;
                else if (canJumpOverWalls && direction == 1) Jump();
            }

            leftWall = Physics2D.Raycast(
                new Vector2(transform.position.x, transform.position.y + rayCastOffset.y),
                Vector2.left, rayCastSize.x, layerMask);
            Debug.DrawRay(
                new Vector2(transform.position.x, transform.position.y + rayCastOffset.y),
                Vector2.left * rayCastSize.x, Color.blue);

            if (leftWall.collider != null)
            {
                if (!isFollowingPlayer) direction = 1;
                else if (canJumpOverWalls && direction == -1) Jump();
            }

            // Ledge detection
            rightLedge = Physics2D.Raycast(
                new Vector2(transform.position.x + rayCastOffset.x, transform.position.y),
                Vector2.down, rayCastSize.y, layerMask);
            Debug.DrawRay(
                new Vector2(transform.position.x + rayCastOffset.x, transform.position.y),
                Vector2.down * rayCastSize.y, Color.blue);

            if ((rightLedge.collider == null || rightLedge.collider.gameObject.layer == 14)
                && direction == 1)
                direction = -1;

            leftLedge = Physics2D.Raycast(
                new Vector2(transform.position.x - rayCastOffset.x, transform.position.y),
                Vector2.down, rayCastSize.y, layerMask);
            Debug.DrawRay(
                new Vector2(transform.position.x - rayCastOffset.x, transform.position.y),
                Vector2.down * rayCastSize.y, Color.blue);

            if ((leftLedge.collider == null || leftLedge.collider.gameObject.layer == 14)
                && direction == -1)
                direction = 1;
        }

        enemyBase.animator.SetBool("grounded", grounded);
        enemyBase.animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / currentMaxSpeed);
        targetVelocity = move * currentMaxSpeed;
    }

    public void Jump()
    {
        if (!grounded) return;
        velocity.y = enemyBase.JumpHeight;
        if (jumpSound != null) enemyBase.audioSource.PlayOneShot(jumpSound);
    }

    public void PlayStepSound()
    {
        if (stepSound == null) return;
        enemyBase.audioSource.pitch = Random.Range(0.6f, 1f);
        enemyBase.audioSource.PlayOneShot(stepSound);
    }

    public void PlayJumpSound()
    {
        if (jumpSound == null) return;
        enemyBase.audioSource.pitch = Random.Range(0.6f, 1f);
        enemyBase.audioSource.PlayOneShot(jumpSound);
    }
}