using UnityEngine;

public class Collectable : MonoBehaviour
{
    private enum ItemType { InventoryItem, Coin, Health }

    [SerializeField] private ItemType itemType;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] collectSounds;
    [SerializeField] private int itemAmount;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite UIImage;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == NewPlayer.Instance.gameObject) Collect();
        if (col.gameObject.layer == 14) Collect(); // Death zone
    }

    public void Collect()
    {
        switch (itemType)
        {
            case ItemType.InventoryItem:
                if (!string.IsNullOrEmpty(itemName))
                    GameManager.Instance.GetInventoryItem(itemName, UIImage);
                break;

            case ItemType.Coin:
                GameManager.Instance.AddCoins(itemAmount);
                break;

            case ItemType.Health:
                HealthComponent playerHealth = NewPlayer.Instance.GetComponent<HealthComponent>();
                if (!playerHealth.IsDead && playerHealth.CurrentHealth < playerHealth.MaxHealth)
                {
                    playerHealth.Heal(itemAmount);
                    GameManager.Instance.hud.HealthBarHurt();
                }
                break;
        }

        GameManager.Instance.audioSource.PlayOneShot(
            collectSounds[Random.Range(0, collectSounds.Length)],
            Random.Range(.6f, 1f));

        if (transform.parent != null && transform.parent.GetComponent<Ejector>() != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
}