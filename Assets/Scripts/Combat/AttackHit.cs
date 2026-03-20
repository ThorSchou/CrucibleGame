using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Place on any trigger collider used as a hitbox � works for both
// player attacks and enemy attacks. Uses IDamageable so it does not
// need to know what it is hitting.
public class AttackHit : MonoBehaviour
{
    private enum AttackTarget { Player, Enemy }

    [SerializeField] private AttackTarget attackTarget;
    [SerializeField] private bool isBomb;
    [SerializeField] private float startCollisionDelay;
    [SerializeField] private GameObject parent;
    [SerializeField] private int hitPower = 1;

    // How long before the same target can be hit again � adjustable per enemy in Inspector
    [SerializeField] private float hitCooldownDuration = 0.5f;

    public List<Collider2D> hitTargets = new List<Collider2D>();
    private float hitCooldown = 0f;

    /// <summary>Called by EnemyBase to sync attack damage each frame.</summary>
    public void SetHitPower(int power) => hitPower = power;

    void Start()
    {
        if (isBomb) StartCoroutine(TempColliderDisable());
    }

    void Update()
    {
        if (hitCooldown > 0f)
            hitCooldown -= Time.deltaTime;
        else
            hitTargets.Clear(); // clear hit list when cooldown expires so targets can be hit again
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (hitCooldown > 0f) return;
        if (hitTargets.Contains(col)) return;

        int direction = parent.transform.position.x < col.transform.position.x ? 1 : -1;
        bool isPlayerCollider = col.GetComponent<NewPlayer>() != null;
        bool shouldHit = (attackTarget == AttackTarget.Player && isPlayerCollider)
                           || (attackTarget == AttackTarget.Enemy && !isPlayerCollider);

        if (shouldHit && col.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(hitPower, direction);
            if (attackTarget == AttackTarget.Enemy && GameManager.Instance != null)
                GameManager.Instance.AddLifetimeDamage(hitPower);
            hitTargets.Add(col);
            hitCooldown = hitCooldownDuration;
            if (isBomb) transform.parent.GetComponent<EnemyBase>().Die();
        }

        // Bombs also detonate on walls (layer 8)
        if (isBomb && col.gameObject.layer == 8)
            transform.parent.GetComponent<EnemyBase>().Die();
    }

    void OnTriggerExit2D(Collider2D col)
    {
        hitTargets.Remove(col);
    }

    private IEnumerator TempColliderDisable()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.enabled = false;
        yield return new WaitForSeconds(startCollisionDelay);
        col.enabled = true;
    }
}