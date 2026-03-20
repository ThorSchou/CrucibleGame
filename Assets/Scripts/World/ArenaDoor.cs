using UnityEngine;

public class ArenaDoor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private SceneLoadTrigger sceneLoadTrigger;
    [SerializeField] private Collider2D physicalCollider; // the non-trigger BoxCollider2D
    [SerializeField] private Collider2D triggerCollider;  // the trigger BoxCollider2D

    private bool isUnlocked = false;
    private EnemySpawner spawner;

    void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>();

        // Start locked — physical wall active, trigger disabled
        if (physicalCollider != null) physicalCollider.enabled = true;
        if (triggerCollider != null) triggerCollider.enabled = false;
    }

    void Update()
    {
        if (isUnlocked) return;

        bool queueExhausted = spawner == null || spawner.IsQueueEmpty;
        bool allEnemiesDead = RoundManager.Instance.EnemiesRemaining == 0;
        bool playerHasKey = GameManager.Instance.inventory.ContainsKey("key");

        if (queueExhausted && allEnemiesDead && playerHasKey)
            Unlock();
    }

    private void Unlock()
    {
        isUnlocked = true;

        // Swap sprites
        if (spriteRenderer != null && unlockedSprite != null)
            spriteRenderer.sprite = unlockedSprite;

        // Disable physical wall so player can walk through
        if (physicalCollider != null) physicalCollider.enabled = false;

        // Enable trigger so scene load fires when player walks through
        if (triggerCollider != null) triggerCollider.enabled = true;

        if (sceneLoadTrigger != null) sceneLoadTrigger.enabled = true;

        Debug.Log("Exit door unlocked!");
    }
}