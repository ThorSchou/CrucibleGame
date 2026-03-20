using UnityEngine;

// General-purpose health component. Attach to player, enemies, and breakables.
// Callers subscribe to OnDeath / OnHurt events instead of polling health values.
public class HealthComponent : MonoBehaviour
{

    [Header("Debug")]
    [SerializeField] private int debugCurrentHealth;
    [SerializeField] private int debugMaxHealth;


    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public event System.Action OnDeath;
    public event System.Action OnHurt;

    public void Initialize(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        debugCurrentHealth = CurrentHealth;
        debugMaxHealth = MaxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        debugCurrentHealth = CurrentHealth;
        OnHurt?.Invoke();
        if (IsDead) OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }

    public void ResetHealth() => Initialize(MaxHealth);
}